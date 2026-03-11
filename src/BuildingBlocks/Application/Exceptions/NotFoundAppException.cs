namespace Application.Exceptions;

public sealed class NotFoundAppException(
    string message,
    string? detail = null,
    string errorCode = "not_found")
    : AppException(404, errorCode, message, detail);
