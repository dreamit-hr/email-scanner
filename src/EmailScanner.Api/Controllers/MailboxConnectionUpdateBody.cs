using EmailScanner.Domain;

namespace EmailScanner.Api.Controllers;

public sealed record MailboxConnectionUpdateBody(string DisplayName, string Folder, MailboxSyncMode SyncMode);
