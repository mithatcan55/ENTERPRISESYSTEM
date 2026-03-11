using Application.Pipeline;
using Identity.Application.Contracts;

namespace Identity.Application.Users.Commands;

public sealed record CreateUserCommand(CreateUserRequest Request) : ITCodeProtectedRequest
{
    public string TransactionCode => "SYS01";
    public string? ActionCode => "CREATE";
}
