namespace Host.Api.Exceptions;

/// <summary>
/// İstenen kaynağın bulunamadığı durumlar için kullanılır.
/// </summary>
public sealed class NotFoundAppException(string message, string? detail = null)
    : AppException(StatusCodes.Status404NotFound, "not_found", message, detail);
