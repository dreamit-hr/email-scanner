using EmailScanner.Domain;

namespace EmailScanner.Application.EmailProcessing.Models;

public sealed class LlmClassificationResponse
{
    public DocumentCategory Category { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<EmailExtractedEntityResult> Entities { get; init; } = [];
}
