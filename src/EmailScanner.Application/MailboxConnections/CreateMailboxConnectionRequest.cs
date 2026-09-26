using EmailScanner.Domain;

namespace EmailScanner.Application.MailboxConnections;

public sealed record CreateMailboxConnectionRequest(Guid TenantId, MailboxProvider Provider, string EmailAddress, string DisplayName, string Folder);
