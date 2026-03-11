using Serilog.Context;

namespace Host.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string CorrelationHeader = "X-Correlation-Id";
    public const string CorrelationItemKey = "CorrelationId";

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[CorrelationItemKey] = correlationId;
        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
