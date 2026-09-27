namespace EmailScanner.Domain;

public sealed class EmailExtractedEntity
{
    private EmailExtractedEntity() { }

    internal EmailExtractedEntity(Guid emailExtractedDocumentId, string entityType, string value, decimal confidence, string source)
    {
        if (emailExtractedDocumentId == Guid.Empty) throw new ArgumentException("Extracted document id is required.", nameof(emailExtractedDocumentId));
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        if (confidence is < 0m or > 1m) throw new ArgumentOutOfRangeException(nameof(confidence));
        Id = Guid.NewGuid();
        EmailExtractedDocumentId = emailExtractedDocumentId;
        EntityType = entityType.Trim();
        Value = value.Trim();
        Confidence = confidence;
        Source = source.Trim();
    }

    public Guid Id { get; private set; }
    public Guid EmailExtractedDocumentId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public decimal Confidence { get; private set; }
    public string Source { get; private set; } = string.Empty;
}
