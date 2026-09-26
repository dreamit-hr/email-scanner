using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class Tenant : AuditableEntity, IAggregateRoot
{
    private readonly List<MailboxConnection> _mailboxes = [];
    private readonly List<EmailRule> _rules = [];
    private readonly List<Webhook> _webhooks = [];

    private Tenant() { }

    public Tenant(string name, string? description)
    {
        Rename(name);
        Description = description;
        IsEnabled = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public IReadOnlyCollection<MailboxConnection> MailboxConnections => _mailboxes.AsReadOnly();
    public IReadOnlyCollection<EmailRule> EmailRules => _rules.AsReadOnly();
    public IReadOnlyCollection<Webhook> Webhooks => _webhooks.AsReadOnly();

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Touch(null);
    }

    public void Enable() { IsEnabled = true; Touch(null); }
    public void Disable() { IsEnabled = false; Touch(null); }
}
