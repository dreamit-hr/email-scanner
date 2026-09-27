namespace EmailScanner.Domain;

public sealed class WatchedSender
{
    private WatchedSender() { }

    public WatchedSender(Guid tenantId, string name, string emailDomain)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = NormalizeName(name);
        EmailDomain = NormalizeDomain(emailDomain);
        IsEnabled = true;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string EmailDomain { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    public void Update(string name, string emailDomain, bool isEnabled)
    {
        Name = NormalizeName(name);
        EmailDomain = NormalizeDomain(emailDomain);
        IsEnabled = isEnabled;
    }

    public static string NormalizeDomain(string emailDomain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailDomain);
        var normalized = emailDomain.Trim().TrimStart('@').TrimEnd('.').ToLowerInvariant();
        if (normalized.Length is 0 or > 255 || normalized.Contains('@') || !normalized.Contains('.') || normalized.Split('.').Any(part => part.Length == 0))
            throw new ArgumentException("A valid email domain is required.", nameof(emailDomain));
        return normalized;
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }
}
