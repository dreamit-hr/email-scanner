namespace EmailScanner.Application.EmailProcessing.Models;

public sealed record EmailAttachmentMetadata(string FileName, string ContentType);

public sealed class EmailClassificationInput
{
    public string Sender { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string QuotedThread { get; init; } = string.Empty;
    public IReadOnlyList<EmailAttachmentMetadata> Attachments { get; init; } = [];
    public IReadOnlyList<EmailClassificationSignal> HeuristicSignals { get; init; } = [];
    public string? OcrText { get; init; }
}
