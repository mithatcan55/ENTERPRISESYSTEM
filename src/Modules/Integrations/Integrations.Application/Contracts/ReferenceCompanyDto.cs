namespace Integrations.Application.Contracts;

public sealed record ReferenceCompanyDto(
    int ExternalId,
    string Name,
    string Email,
    string SourceSystem);
