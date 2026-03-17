using System.Text.Json;
using Infrastructure.Persistence;
using Npgsql;
using Serilog.Core;
using Serilog.Events;

namespace Host.Api.Logging;

/// <summary>
/// Serilog olaylarini mevcut system_logs tablosuna yazar.
/// Hazir PostgreSQL sink yerine bu sinifin kullanilma nedeni, mevcut log semamizla
/// birebir uyumlu kalmak ve ikinci bir "generic logs" tablosu olusturmamaktir.
/// </summary>
public sealed class PostgreSqlSystemLogSink(string connectionString, IHostEnvironment environment) : ILogEventSink
{
    private const string InsertSql = """
        INSERT INTO logs.system_logs
        (
            "Timestamp", "TimeZone", "Level", "Category", "Source", "Message", "MessageTemplate",
            "Exception", "StackTrace", "CorrelationId", "RequestId", "SessionId", "HttpMethod",
            "HttpPath", "HttpStatusCode", "HttpDurationMs", "OperationName", "DurationMs",
            "MachineName", "Environment", "ApplicationName", "ApplicationVersion",
            "ProcessId", "ThreadId", "Properties"
        )
        VALUES
        (
            @timestamp, @timeZone, @level, @category, @source, @message, @messageTemplate,
            @exception, @stackTrace, @correlationId, @requestId, @sessionId, @httpMethod,
            @httpPath, @httpStatusCode, @httpDurationMs, @operationName, @durationMs,
            @machineName, @environment, @applicationName, @applicationVersion,
            @processId, @threadId, @properties
        );
        """;

    public void Emit(LogEvent logEvent)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using var command = new NpgsqlCommand(InsertSql, connection);
            command.Parameters.AddWithValue("timestamp", logEvent.Timestamp);
            command.Parameters.AddWithValue("timeZone", TimeZoneInfo.Local.Id);
            command.Parameters.AddWithValue("level", logEvent.Level.ToString());
            command.Parameters.AddWithValue("category", ResolveCategory(logEvent));
            command.Parameters.AddWithValue("source", GetScalarString(logEvent, "SourceContext") ?? "Serilog");
            command.Parameters.AddWithValue("message", logEvent.RenderMessage());
            command.Parameters.AddWithValue("messageTemplate", logEvent.MessageTemplate.Text);
            command.Parameters.AddWithValue("exception", (object?)logEvent.Exception?.Message ?? DBNull.Value);
            command.Parameters.AddWithValue("stackTrace", (object?)logEvent.Exception?.ToString() ?? DBNull.Value);
            command.Parameters.AddWithValue("correlationId", (object?)GetScalarString(logEvent, "CorrelationId") ?? DBNull.Value);
            command.Parameters.AddWithValue("requestId", (object?)GetScalarString(logEvent, "RequestId") ?? DBNull.Value);
            command.Parameters.AddWithValue("sessionId", (object?)GetScalarString(logEvent, "SessionId") ?? DBNull.Value);
            command.Parameters.AddWithValue("httpMethod", (object?)GetScalarString(logEvent, "RequestMethod") ?? DBNull.Value);
            command.Parameters.AddWithValue("httpPath", (object?)GetScalarString(logEvent, "RequestPath") ?? DBNull.Value);
            command.Parameters.AddWithValue("httpStatusCode", GetNullableInt(logEvent, "StatusCode") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("httpDurationMs", GetNullableLong(logEvent, "Elapsed") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("operationName", (object?)GetScalarString(logEvent, "OperationName") ?? DBNull.Value);
            command.Parameters.AddWithValue("durationMs", GetNullableLong(logEvent, "DurationMs") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("machineName", Environment.MachineName);
            command.Parameters.AddWithValue("environment", environment.EnvironmentName);
            command.Parameters.AddWithValue("applicationName", environment.ApplicationName);
            command.Parameters.AddWithValue("applicationVersion", typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");
            command.Parameters.AddWithValue("processId", Environment.ProcessId);
            command.Parameters.AddWithValue("threadId", Environment.CurrentManagedThreadId);
            command.Parameters.AddWithValue("properties", SerializeProperties(logEvent));

            command.ExecuteNonQuery();
        }
        catch
        {
            // Sink icindeki hata tekrar loglanirsa recursive log firtinasi olusur.
            // Bu nedenle burada sessizce dusurup sistemin calismasina izin veriyoruz.
        }
    }

    private static string ResolveCategory(LogEvent logEvent)
    {
        var sourceContext = GetScalarString(logEvent, "SourceContext");
        if (string.IsNullOrWhiteSpace(sourceContext))
        {
            return "System";
        }

        if (sourceContext.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal))
        {
            return "AspNetCore";
        }

        if (sourceContext.StartsWith("Microsoft.Hosting", StringComparison.Ordinal))
        {
            return "Hosting";
        }

        return "Application";
    }

    private static string SerializeProperties(LogEvent logEvent)
    {
        var properties = logEvent.Properties.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToString());

        return JsonSerializer.Serialize(properties);
    }

    private static string? GetScalarString(LogEvent logEvent, string propertyName)
    {
        return logEvent.Properties.TryGetValue(propertyName, out var value)
            ? TrimQuotes(value.ToString())
            : null;
    }

    private static int? GetNullableInt(LogEvent logEvent, string propertyName)
    {
        if (!logEvent.Properties.TryGetValue(propertyName, out var value))
        {
            return null;
        }

        return int.TryParse(TrimQuotes(value.ToString()), out var parsed)
            ? parsed
            : null;
    }

    private static long? GetNullableLong(LogEvent logEvent, string propertyName)
    {
        if (!logEvent.Properties.TryGetValue(propertyName, out var value))
        {
            return null;
        }

        if (long.TryParse(TrimQuotes(value.ToString()), out var parsedLong))
        {
            return parsedLong;
        }

        if (double.TryParse(TrimQuotes(value.ToString()), out var parsedDouble))
        {
            return (long)Math.Round(parsedDouble);
        }

        return null;
    }

    private static string TrimQuotes(string value)
        => value.Length >= 2 && value[0] == '"' && value[^1] == '"'
            ? value[1..^1]
            : value;
}
