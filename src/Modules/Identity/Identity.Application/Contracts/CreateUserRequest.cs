namespace Identity.Application.Contracts;

public sealed class CreateUserRequest
{
    public string UserCode { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public bool NotifyAdminByMail { get; set; }
    public string? AdminEmail { get; set; }
}
