namespace EmailScanner.Application.Abstractions;

public sealed record ApiClientInfo(string ClientId, Guid TenantId, IReadOnlySet<string> Permissions);
