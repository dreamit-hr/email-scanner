using System.Security.Cryptography;
using EmailScanner.Domain;
using EmailScanner.Infrastructure.BlobStorage;
using EmailScanner.Infrastructure.ValKey;
using EmailScanner.Repository;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

internal sealed class ImapMailboxSyncService(
    EmailScannerDbContext dbContext,
    IDistributedLockService distributedLock,
    IBlobStorageClient blobStorage,
    BlobStoragePathProvider pathProvider,
    IConfiguration configuration,
    ILogger<ImapMailboxSyncService> logger)
{
    public async Task SyncAsync(Guid mailboxId, CancellationToken cancellationToken)
    {
        await using var lease = await distributedLock.TryAcquireAsync(
            $"mailbox-sync:{mailboxId:D}", TimeSpan.FromMinutes(10), cancellationToken);
        if (lease is null)
        {
            logger.LogDebug("Mailbox {MailboxId} is already being synchronized by another worker.", mailboxId);
            return;
        }

        var mailbox = await dbContext.MailboxConnection
            .SingleOrDefaultAsync(x => x.Id == mailboxId && x.Provider == MailboxProvider.Imap, cancellationToken);
        if (mailbox is null || mailbox.Status != MailboxConnectionStatus.Enabled)
            return;

        try
        {
            if (string.IsNullOrWhiteSpace(mailbox.ImapHost)
                || string.IsNullOrWhiteSpace(mailbox.ImapUsername)
                || string.IsNullOrWhiteSpace(mailbox.ImapCredentialReference))
                throw new InvalidOperationException("IMAP host, username, and password are required for this mailbox.");

            mailbox.MarkSyncStarted(DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            using var client = new ImapClient();
            await client.ConnectAsync(mailbox.ImapHost, mailbox.ImapPort ?? 993, SecureSocketOptions.SslOnConnect, cancellationToken);
            await client.AuthenticateAsync(mailbox.ImapUsername, mailbox.ImapCredentialReference, cancellationToken);

            var folder = client.Inbox;
            await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            //var folder = client.GetFolder(mailbox.Folder);
            //await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            var (tokenUidValidity, tokenLastUid) = ParseSyncToken(mailbox.SyncToken);
            var lastUid = tokenUidValidity == folder.UidValidity
                ? tokenLastUid
                : 0u;
            var lookbackDays = int.TryParse(configuration["MailboxSync:LookbackDays"], out var configuredLookbackDays)
                               && configuredLookbackDays > 0
                ? configuredLookbackDays
                : 10;
            var receivedSince = DateTime.UtcNow.AddDays(-lookbackDays);
            var messageUids = await folder.SearchAsync(SearchQuery.DeliveredAfter(receivedSince), cancellationToken);

            foreach (var uid in messageUids.Where(uid => uid.Id > lastUid).OrderBy(uid => uid.Id))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ImportMessageAsync(mailbox, folder, uid, cancellationToken);

                lastUid = uid.Id;
                mailbox.UpdateSyncToken(FormatSyncToken(folder.UidValidity, lastUid));
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            mailbox.MarkSyncCompleted(DateTime.UtcNow, FormatSyncToken(folder.UidValidity, lastUid));
            await dbContext.SaveChangesAsync(cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            logger.LogInformation("IMAP synchronization completed for mailbox {MailboxId}; latest UID is {LastUid}.", mailboxId, lastUid);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            mailbox.MarkSyncFailed(DateTime.UtcNow);
            await dbContext.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task ImportMessageAsync(MailboxConnection mailbox, IMailFolder folder, UniqueId uid, CancellationToken cancellationToken)
    {
        var message = await folder.GetMessageAsync(uid, cancellationToken);
        var internetMessageId = string.IsNullOrWhiteSpace(message.MessageId)
            ? $"imap-{folder.UidValidity}-{uid.Id}@emailscanner.local"
            : message.MessageId;

        if (await dbContext.Email.AnyAsync(
                email => email.MailboxConnectionId == mailbox.Id && email.InternetMessageId == internetMessageId,
                cancellationToken))
            return;

        var sender = message.From.Mailboxes.FirstOrDefault()?.Address ?? mailbox.EmailAddress;
        var receivedUtc = message.Date == DateTimeOffset.MinValue ? DateTime.UtcNow : message.Date.UtcDateTime;
        await using var rawMime = new MemoryStream();
        await message.WriteToAsync(rawMime, cancellationToken);
        var rawBytes = rawMime.ToArray();
        var mimeHash = Convert.ToHexString(SHA256.HashData(rawBytes)).ToLowerInvariant();
        var emailId = Guid.NewGuid();
        var mimeBlobPath = pathProvider.EmailMime(mailbox.TenantId, mailbox.Id, emailId);

        await using (var content = new MemoryStream(rawBytes, writable: false))
            await blobStorage.UploadAsync(mimeBlobPath, content, "message/rfc822", cancellationToken);

        var email = new Email(
            mailbox.Id,
            internetMessageId,
            sender,
            message.Subject ?? string.Empty,
            message.TextBody ?? message.HtmlBody ?? string.Empty,
            receivedUtc,
            mimeBlobPath,
            mimeHash,
            emailId);

        AddRecipients(email, message.To, RecipientType.To);
        AddRecipients(email, message.Cc, RecipientType.Cc);
        AddRecipients(email, message.Bcc, RecipientType.Bcc);
        await StoreAttachmentsAsync(email, message, cancellationToken);
        dbContext.Email.Add(email);
    }

    private async Task StoreAttachmentsAsync(Email email, MimeMessage message, CancellationToken cancellationToken)
    {
        foreach (var mimePart in message.Attachments.OfType<MimePart>())
        {
            if (mimePart.Content is null) continue;
            await using var content = new MemoryStream();
            await mimePart.Content.DecodeToAsync(content, cancellationToken);
            var bytes = content.ToArray();
            var attachmentId = Guid.NewGuid();
            var fileName = string.IsNullOrWhiteSpace(mimePart.FileName) ? $"attachment-{attachmentId:N}.bin" : mimePart.FileName;
            var blobPath = pathProvider.Attachment(attachmentId, fileName);
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            await using (var upload = new MemoryStream(bytes, writable: false))
                await blobStorage.UploadAsync(blobPath, upload, mimePart.ContentType.MimeType, cancellationToken);
            email.AddAttachment(new EmailScanner.Domain.Attachment(
                email.Id,
                fileName,
                mimePart.ContentType.MimeType,
                bytes.LongLength,
                hash,
                blobPath));
        }
    }

    private static void AddRecipients(Email email, InternetAddressList addresses, RecipientType recipientType)
    {
        foreach (var address in addresses.Mailboxes)
        {
            try
            {
                email.AddRecipient(new EmailRecipient(email.Id, recipientType, address.Address, address.Name));
            }
            catch (ArgumentException)
            {
                // Ignore malformed recipient addresses while preserving the original MIME message.
            }
        }
    }

    private static (uint UidValidity, uint LastUid) ParseSyncToken(string? token)
    {
        var parts = token?.Split(':', 2);
        return parts is { Length: 2 }
               && uint.TryParse(parts[0], out var uidValidity)
               && uint.TryParse(parts[1], out var lastUid)
            ? (uidValidity, lastUid)
            : (0, 0);
    }

    private static string FormatSyncToken(uint uidValidity, uint lastUid) =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{uidValidity}:{lastUid}");
}
