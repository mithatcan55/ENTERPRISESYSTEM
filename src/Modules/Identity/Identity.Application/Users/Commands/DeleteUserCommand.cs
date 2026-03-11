using Application.Pipeline;

namespace Identity.Application.Users.Commands;

public sealed record DeleteUserCommand(int UserId) : ITCodeProtectedRequest
{
    public string TransactionCode => "SYS01";
    public string? ActionCode => "DELETE";
}
