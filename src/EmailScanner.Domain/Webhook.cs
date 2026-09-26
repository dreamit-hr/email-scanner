using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class Webhook : AuditableEntity, IAggregateRoot
{
    private Webhook() { }
    public Webhook(Guid tenantId, string name, Uri url, string secret)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!url.IsAbsoluteUri || url.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Webhook URL must use HTTPS.", nameof(url));
        TenantId = tenantId; Name = name.Trim(); Url = url; RotateSecret(secret); IsEnabled = true;
    }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Uri Url { get; private set; } = new("https://localhost");
    public string Secret { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public void Enable() { IsEnabled = true; Touch(null); }
    public void Disable() { IsEnabled = false; Touch(null); }
    public void RotateSecret(string secret) { ArgumentException.ThrowIfNullOrWhiteSpace(secret); Secret = secret; Touch(null); }
}
