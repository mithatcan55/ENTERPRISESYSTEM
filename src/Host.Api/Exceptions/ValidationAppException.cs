namespace Host.Api.Exceptions;

/// <summary>
/// İş/DTO doğrulama hataları için kullanılır.
/// </summary>
public sealed class ValidationAppException(
    string message,
    IReadOnlyDictionary<string, string[]> errors,
    string? detail = null)
    : AppException(StatusCodes.Status400BadRequest, "validation_error", message, detail, errors);
