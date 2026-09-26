namespace EmailScanner.Infrastructure.BlobStorage;

public sealed class BlobStoragePathProvider
{
    public string EmailMime(Guid tenantId, Guid mailboxConnectionId, Guid emailId) => $"emails/{tenantId:D}/{mailboxConnectionId:D}/{emailId:D}/original.eml";
    public string Attachment(Guid attachmentId, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var safeFileName = Path.GetFileName(fileName).Replace('/', '_').Replace('\\', '_');
        return $"attachments/{attachmentId:D}/{Uri.EscapeDataString(safeFileName)}";
    }
}
