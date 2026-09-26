using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class EmailRule : AuditableEntity, IAggregateRoot
{
    private readonly List<EmailRuleCondition> _conditions = [];
    private readonly List<EmailRuleAction> _actions = [];
    private EmailRule() { }
    public EmailRule(Guid tenantId, string name, int priority)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        TenantId = tenantId; Name = name.Trim(); Priority = priority; IsEnabled = true;
    }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Priority { get; private set; }
    public bool IsEnabled { get; private set; }
    public IReadOnlyCollection<EmailRuleCondition> Conditions => _conditions.AsReadOnly();
    public IReadOnlyCollection<EmailRuleAction> Actions => _actions.AsReadOnly();
    public void Enable() { IsEnabled = true; Touch(null); }
    public void Disable() { IsEnabled = false; Touch(null); }
    public void AddCondition(RuleOperator op, string property, string value) { _conditions.Add(new EmailRuleCondition(Id, op, property, value)); Touch(null); }
    public void AddAction(RuleActionType type, string configurationJson) { _actions.Add(new EmailRuleAction(Id, type, configurationJson)); Touch(null); }
}
