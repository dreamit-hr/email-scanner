using System.Text.RegularExpressions;
using EmailScanner.Domain;

namespace EmailScanner.Application.Rules;

public sealed class RuleEngine
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

    public IReadOnlyList<EmailRule> Match(Email email, IEnumerable<EmailRule> rules)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(rules);
        return rules.Where(rule => rule.IsEnabled && rule.Conditions.All(condition => Matches(email, condition)))
            .OrderBy(rule => rule.Priority).ToArray();
    }

    private static bool Matches(Email email, EmailRuleCondition condition)
    {
        if (condition.Operator == RuleOperator.AttachmentExists) return email.HasAttachments;
        var value = GetValue(email, condition.Property);
        if (value is null) return false;
        try
        {
            return condition.Operator switch
            {
                RuleOperator.Contains => value.Contains(condition.Value, StringComparison.OrdinalIgnoreCase),
                RuleOperator.Equals => string.Equals(value, condition.Value, StringComparison.OrdinalIgnoreCase),
                RuleOperator.StartsWith => value.StartsWith(condition.Value, StringComparison.OrdinalIgnoreCase),
                RuleOperator.EndsWith => value.EndsWith(condition.Value, StringComparison.OrdinalIgnoreCase),
                RuleOperator.Regex => Regex.IsMatch(value, condition.Value, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexTimeout),
                RuleOperator.SenderDomain => SenderDomainMatches(value, condition.Value),
                _ => false
            };
        }
        catch (ArgumentException) { return false; }
        catch (RegexMatchTimeoutException) { return false; }
    }

    private static string? GetValue(Email email, string property) => property.Trim().ToLowerInvariant() switch
    {
        "sender" => email.Sender,
        "senderdomain" => email.Sender[(email.Sender.LastIndexOf('@') + 1)..],
        "subject" => email.Subject,
        "body" => email.Body,
        "filename" => string.Join('\n', email.Attachments.Select(x => x.FileName)),
        "mimetype" => string.Join('\n', email.Attachments.Select(x => x.ContentType)),
        _ => null
    };

    private static bool SenderDomainMatches(string actual, string expected) =>
        string.Equals(actual.Trim().TrimStart('@'), expected.Trim().TrimStart('@'), StringComparison.OrdinalIgnoreCase);
}
