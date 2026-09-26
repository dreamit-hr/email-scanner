using EmailScanner.Application.Rules;
using EmailScanner.Domain;
using FluentAssertions;
using Xunit;

namespace EmailScanner.Application.Tests;

public sealed class RuleEngineTests
{
    [Fact]
    public void Match_returns_enabled_rules_in_priority_order()
    {
        var email = new Email(Guid.NewGuid(), "<one@example.com>", "billing@vendor.example", "Invoice 2026", "Please review", DateTime.UtcNow, "mail/one.eml", "abc");
        var lowPriority = new EmailRule(Guid.NewGuid(), "subject", 10);
        lowPriority.AddCondition(RuleOperator.Contains, "Subject", "invoice");
        var highPriority = new EmailRule(Guid.NewGuid(), "sender", 2);
        highPriority.AddCondition(RuleOperator.SenderDomain, "SenderDomain", "vendor.example");
        var disabled = new EmailRule(Guid.NewGuid(), "disabled", 1);
        disabled.Disable();
        new RuleEngine().Match(email, [lowPriority, disabled, highPriority]).Should().ContainInOrder(highPriority, lowPriority);
    }

    [Fact]
    public void Invalid_regular_expression_does_not_break_rule_processing()
    {
        var email = new Email(Guid.NewGuid(), "id", "user@example.com", "Subject", "Body", DateTime.UtcNow, "mail/one.eml", "abc");
        var rule = new EmailRule(Guid.NewGuid(), "bad regex", 1);
        rule.AddCondition(RuleOperator.Regex, "Subject", "[");
        new RuleEngine().Match(email, [rule]).Should().BeEmpty();
    }
}
