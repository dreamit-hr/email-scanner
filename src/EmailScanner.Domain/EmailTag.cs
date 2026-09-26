using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class EmailTag : AuditableEntity
{
    private EmailTag() { }
    public EmailTag(Guid tenantId, string name, string color)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        TenantId = tenantId; Name = name.Trim(); Color = color ?? string.Empty;
    }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
}
