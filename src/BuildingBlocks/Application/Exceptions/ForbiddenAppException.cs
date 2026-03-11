namespace Application.Exceptions;

public sealed class ForbiddenAppException(
    string message,
    string? detail = null,
    string errorCode = "forbidden")
    : AppException(403, errorCode, message, detail);
