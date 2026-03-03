namespace Host.Api.Exceptions;

/// <summary>
/// Uygulama seviyesindeki bilinen hatalar için temel sınıf.
/// Global exception handler bu sınıftan türeyen hataları standart ProblemDetails formatında döner.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(
        int statusCode,
        string errorCode,
        string message,
        string? detail = null,
        IReadOnlyDictionary<string, string[]>? errors = null,
        Exception? innerException = null) : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Detail = detail;
        Errors = errors;
    }

    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string? Detail { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
