using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

/// <summary>
/// Captures executed SQL commands from BusinessDbContext and stores them
/// in LogDb (database_query_logs) and application txt logs via ILogger.
/// </summary>
public sealed class DatabaseCommandLoggingInterceptor(
    IServiceScopeFactory serviceScopeFactory,
    IAuditActorAccessor auditActorAccessor,
    ILogger<DatabaseCommandLoggingInterceptor> logger) : DbCommandInterceptor
{
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        WriteLog(command, eventData, rowsAffected: -1, isError: false, errorMessage: null, CancellationToken.None);
        return result;
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        await WriteLogAsync(command, eventData, rowsAffected: -1, isError: false, errorMessage: null, cancellationToken);
        return result;
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        WriteLog(command, eventData, rowsAffected: -1, isError: false, errorMessage: null, CancellationToken.None);
        return result;
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        await WriteLogAsync(command, eventData, rowsAffected: -1, isError: false, errorMessage: null, cancellationToken);
        return result;
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        WriteLog(command, eventData, rowsAffected: result, isError: false, errorMessage: null, CancellationToken.None);
        return result;
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await WriteLogAsync(command, eventData, rowsAffected: result, isError: false, errorMessage: null, cancellationToken);
        return result;
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        WriteLog(command, eventData, rowsAffected: 0, isError: true, errorMessage: eventData.Exception.Message, CancellationToken.None);
        base.CommandFailed(command, eventData);
    }

    public override async Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await WriteLogAsync(command, eventData, rowsAffected: 0, isError: true, errorMessage: eventData.Exception.Message, cancellationToken);
        await base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void WriteLog(
        DbCommand command,
        CommandEventData eventData,
        int rowsAffected,
        bool isError,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            WriteLogAsync(command, eventData, rowsAffected, isError, errorMessage, cancellationToken).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DatabaseQueryLog sync write failed.");
        }
    }

    private async Task WriteLogAsync(
        DbCommand command,
        CommandEventData eventData,
        int rowsAffected,
        bool isError,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        // Ignore log-db commands to prevent accidental recursion/noise.
        if (command.CommandText.Contains("logs.database_query_logs", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var operation = ResolveOperation(command.CommandText);
        var tableName = ResolveTableName(command.CommandText, operation);
        var parameters = SerializeParameters(command);
        var correlationId = Activity.Current?.Id;
        var userId = SafeGetActorId();
        var durationMs = eventData switch
        {
            CommandExecutedEventData executed => (long)Math.Max(0, executed.Duration.TotalMilliseconds),
            CommandErrorEventData failed => (long)Math.Max(0, failed.Duration.TotalMilliseconds),
            _ => 0
        };

        var logEntity = new DatabaseQueryLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            CorrelationId = correlationId,
            UserId = userId,
            Operation = operation,
            TableName = tableName,
            CommandText = command.CommandText,
            Parameters = parameters,
            DurationMs = durationMs,
            RowsAffected = rowsAffected,
            IsError = isError,
            ErrorMessage = errorMessage
        };

        var systemLog = new SystemLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            TimeZone = "UTC",
            Level = isError ? "Error" : "Information",
            Category = "DatabaseQuery",
            Source = nameof(DatabaseCommandLoggingInterceptor),
            Message = isError
                ? $"SQL command failed. Operation={operation}, Table={tableName}, DurationMs={durationMs}"
                : $"SQL command executed. Operation={operation}, Table={tableName}, DurationMs={durationMs}, RowsAffected={rowsAffected}",
            UserId = userId,
            CorrelationId = correlationId,
            DbOperation = operation,
            DbTable = tableName,
            DbCommand = command.CommandText,
            DbParameters = parameters,
            DbDurationMs = durationMs,
            DbRowsAffected = rowsAffected,
            OperationName = operation,
            DurationMs = durationMs,
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId
        };

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var logDbContext = scope.ServiceProvider.GetRequiredService<LogDbContext>();
        logDbContext.DatabaseQueryLogs.Add(logEntity);
        logDbContext.SystemLogs.Add(systemLog);
        await logDbContext.SaveChangesAsync(cancellationToken);

        if (isError)
        {
            logger.LogError(
                "SQL command failed. Operation={Operation}, Table={Table}, DurationMs={DurationMs}, Error={Error}",
                operation,
                tableName,
                durationMs,
                errorMessage);
        }
        else
        {
            logger.LogInformation(
                "SQL command executed. Operation={Operation}, Table={Table}, DurationMs={DurationMs}, RowsAffected={RowsAffected}",
                operation,
                tableName,
                durationMs,
                rowsAffected);
        }
    }

    private string SafeGetActorId()
    {
        try
        {
            return auditActorAccessor.GetActorId();
        }
        catch
        {
            return "system";
        }
    }

    private static string ResolveOperation(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return "UNKNOWN";
        }

        var token = sql.TrimStart().Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return token?.ToUpperInvariant() ?? "UNKNOWN";
    }

    private static string? ResolveTableName(string sql, string operation)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return null;
        }

        const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
        var pattern = operation switch
        {
            "SELECT" => @"\bFROM\s+(?<table>[^\s,;()]+)",
            "INSERT" => @"\bINTO\s+(?<table>[^\s,;()]+)",
            "UPDATE" => @"\bUPDATE\s+(?<table>[^\s,;()]+)",
            "DELETE" => @"\bFROM\s+(?<table>[^\s,;()]+)",
            "CREATE" => @"\bTABLE\s+(?<table>[^\s,;()]+)",
            "ALTER" => @"\bTABLE\s+(?<table>[^\s,;()]+)",
            "DROP" => @"\bTABLE\s+(?<table>[^\s,;()]+)",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return null;
        }

        var match = Regex.Match(sql, pattern, regexOptions);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups["table"].Value.Trim('"');
    }

    private static string? SerializeParameters(DbCommand command)
    {
        if (command.Parameters.Count == 0)
        {
            return null;
        }

        var payload = new List<object?>(command.Parameters.Count);
        foreach (DbParameter parameter in command.Parameters)
        {
            payload.Add(new
            {
                parameter.ParameterName,
                Value = parameter.Value,
                parameter.DbType,
                parameter.Direction
            });
        }

        return JsonSerializer.Serialize(payload);
    }
}
