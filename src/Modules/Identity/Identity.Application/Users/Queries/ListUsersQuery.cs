using Application.Pipeline;

namespace Identity.Application.Users.Queries;

public sealed record ListUsersQuery : ITCodeProtectedRequest
{
    public string TransactionCode => "SYS04";
    public string? ActionCode => "READ";

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
    public bool? IsActive { get; init; }
    public bool IncludeDeleted { get; init; } = false;
}
