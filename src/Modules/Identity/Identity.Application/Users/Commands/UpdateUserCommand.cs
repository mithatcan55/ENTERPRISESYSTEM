using Application.Pipeline;
using Identity.Application.Contracts;

namespace Identity.Application.Users.Commands;

public sealed record UpdateUserCommand(int UserId, UpdateUserRequest Request) : ITCodeProtectedRequest
{
    public string TransactionCode => "SYS01";
    public string? ActionCode => "UPDATE";
}
