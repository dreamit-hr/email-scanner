using EmailScanner.Application.EmailProcessing.Models;
using EmailScanner.Domain;

namespace EmailScanner.Application.EmailProcessing;

public interface IEmailClassificationEngine
{
    Task<EmailClassificationResult> ClassifyAsync(
        Email email,
        IReadOnlyList<EmailAttachmentMetadata> attachments,
        IReadOnlyList<WatchedSender> watchedSenders,
        string? ocrText = null,
        CancellationToken cancellationToken = default);
}
