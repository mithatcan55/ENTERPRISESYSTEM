namespace Application.Exceptions;

public sealed class ValidationAppException(
    string message,
    IReadOnlyDictionary<string, string[]> errors,
    string? detail = null,
    string errorCode = "validation_error")
    : AppException(400, errorCode, message, detail, errors);
