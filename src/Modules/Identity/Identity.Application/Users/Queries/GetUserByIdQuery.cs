using Application.Pipeline;

namespace Identity.Application.Users.Queries;

public sealed record GetUserByIdQuery(int UserId) : ITCodeProtectedRequest
{
    public string TransactionCode => "SYS03";
    public string? ActionCode => "READ";
}
