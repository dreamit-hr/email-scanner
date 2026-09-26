using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class EmailRuleAction : BaseEntity
{
    private EmailRuleAction() { }
    internal EmailRuleAction(Guid emailRuleId, RuleActionType type, string configurationJson)
    {
        EmailRuleId = emailRuleId; ActionType = type; ConfigurationJson = configurationJson ?? "{}";
        System.Text.Json.JsonDocument.Parse(ConfigurationJson).Dispose();
    }
    public Guid EmailRuleId { get; private set; }
    public RuleActionType ActionType { get; private set; }
    public string ConfigurationJson { get; private set; } = "{}";
}
