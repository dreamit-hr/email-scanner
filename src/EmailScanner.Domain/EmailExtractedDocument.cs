namespace EmailScanner.Domain;

public sealed class EmailExtractedDocument
{
    private readonly List<EmailExtractedEntity> _entities = [];

    private EmailExtractedDocument() { }

    public EmailExtractedDocument(Guid tenantId, Guid emailId, DocumentCategory category, string documentType, decimal confidence, string summary)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        if (emailId == Guid.Empty) throw new ArgumentException("Email id is required.", nameof(emailId));
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType);
        if (confidence is < 0m or > 1m) throw new ArgumentOutOfRangeException(nameof(confidence));
        Id = Guid.NewGuid();
        TenantId = tenantId;
        EmailId = emailId;
        Category = category;
        DocumentType = documentType.Trim();
        Confidence = confidence;
        Summary = summary ?? string.Empty;
        CreatedUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid EmailId { get; private set; }
    public DocumentCategory Category { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public decimal Confidence { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public DateTime CreatedUtc { get; private set; }
    public IReadOnlyCollection<EmailExtractedEntity> Entities => _entities.AsReadOnly();

    public void AddEntity(string entityType, string value, decimal confidence, string source)
    {
        _entities.Add(new EmailExtractedEntity(Id, entityType, value, confidence, source));
    }
}
