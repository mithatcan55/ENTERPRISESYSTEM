namespace Host.Api.Exceptions;

/// <summary>
/// Kullanıcının kaynak/işlem için yetkisi olmadığı durumlarda kullanılır.
/// </summary>
public sealed class ForbiddenAppException(string message, string? detail = null)
    : AppException(StatusCodes.Status403Forbidden, "forbidden", message, detail);
