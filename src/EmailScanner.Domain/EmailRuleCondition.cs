using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class EmailRuleCondition : BaseEntity
{
    private EmailRuleCondition() { }
    internal EmailRuleCondition(Guid emailRuleId, RuleOperator op, string property, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(property);
        EmailRuleId = emailRuleId; Operator = op; Property = property.Trim(); Value = value ?? string.Empty;
    }
    public Guid EmailRuleId { get; private set; }
    public RuleOperator Operator { get; private set; }
    public string Property { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
}
