using EmailScanner.Domain;

namespace EmailScanner.Application.EmailProcessing.Models;

public sealed class EmailClassificationResult
{
    public bool IsCandidate { get; init; }
    public DocumentCategory Category { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? IgnoreReason { get; init; }
    public ICollection<EmailClassificationSignal> Signals { get; init; } = [];
    public ICollection<EmailExtractedEntityResult> Entities { get; init; } = [];
}
