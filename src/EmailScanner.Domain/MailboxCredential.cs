using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class MailboxCredential : AuditableEntity
{
    private MailboxCredential() { }
    public MailboxCredential(Guid tenantId, MailboxProvider provider, string credentialReference, string credentialName)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialName);
        TenantId = tenantId; Provider = provider; CredentialReference = credentialReference.Trim(); CredentialName = credentialName.Trim();
    }
    public Guid TenantId { get; private set; }
    public MailboxProvider Provider { get; private set; }
    public string CredentialReference { get; private set; } = string.Empty;
    public string CredentialName { get; private set; } = string.Empty;
}
