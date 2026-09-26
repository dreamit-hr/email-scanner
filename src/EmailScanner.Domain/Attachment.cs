using EmailScanner.Domain.Abstractions;
using EmailScanner.Domain.Events;

namespace EmailScanner.Domain;

public sealed class Attachment : AuditableEntity
{
    private Attachment() { }
    public Attachment(Guid emailId, string fileName, string contentType, long fileSize, string hash, string blobPath)
    {
        if (emailId == Guid.Empty) throw new ArgumentException("Email id is required.", nameof(emailId));
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName); ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        if (fileSize < 0) throw new ArgumentOutOfRangeException(nameof(fileSize));
        EmailId = emailId; FileName = Path.GetFileName(fileName); ContentType = contentType; FileSize = fileSize; Hash = hash; BlobPath = blobPath;
    }
    public Guid EmailId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string Hash { get; private set; } = string.Empty;
    public string BlobPath { get; private set; } = string.Empty;
    public AttachmentStatus Status { get; private set; } = AttachmentStatus.Pending;
    public void MarkUploaded() { Status = AttachmentStatus.Uploaded; Touch(null); AddDomainEvent(new AttachmentUploadedDomainEvent(Id, DateTime.UtcNow)); }
    public void MarkProcessed() { Status = AttachmentStatus.Processed; Touch(null); }
    public void MarkFailed() { Status = AttachmentStatus.Failed; Touch(null); }
}
