namespace Host.Api.Identity.Contracts;

public sealed class CreateUserRequest
{
    public string UserCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public bool NotifyAdminByMail { get; set; }
    public string? AdminEmail { get; set; }
}
