namespace Host.Api.Integrations.Contracts;

public sealed record ReferenceCompanyDto(
    int ExternalId,
    string Name,
    string Email,
    string SourceSystem);
