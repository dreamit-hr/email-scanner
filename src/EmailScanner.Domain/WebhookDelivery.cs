using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class WebhookDelivery : AuditableEntity
{
    private WebhookDelivery() { }
    public WebhookDelivery(Guid webhookId, Guid emailId, WebhookEventType eventType)
    {
        if (webhookId == Guid.Empty || emailId == Guid.Empty) throw new ArgumentException("Webhook and email ids are required.");
        WebhookId = webhookId; EmailId = emailId; EventType = eventType; Attempt = 0;
    }
    public Guid WebhookId { get; private set; }
    public Guid EmailId { get; private set; }
    public WebhookEventType EventType { get; private set; }
    public int Attempt { get; private set; }
    public int? StatusCode { get; private set; }
    public string? ResponseBody { get; private set; }
    public DateTime? ExecutedUtc { get; private set; }
    public long? DurationMs { get; private set; }
    public void RecordAttempt(int? statusCode, string? responseBody, DateTime executedUtc, long durationMs)
    {
        if (executedUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("Timestamp must be UTC.", nameof(executedUtc));
        Attempt++; StatusCode = statusCode; ResponseBody = responseBody; ExecutedUtc = executedUtc; DurationMs = durationMs; Touch(null);
    }
}
