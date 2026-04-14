namespace Authorization.Application.Contracts;

public sealed record TCodeNavigationItemDto(
    string TransactionCode,
    string Name,
    string RouteLink,
    string? SourceTransactionCode = null);
