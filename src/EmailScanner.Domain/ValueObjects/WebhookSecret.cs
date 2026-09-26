using System.Security.Cryptography;

namespace EmailScanner.Domain.ValueObjects;

public sealed record WebhookSecret
{
    private WebhookSecret(string value) => Value = value;
    public string Value { get; }
    public static WebhookSecret Generate() => new(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    public static WebhookSecret Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length < 32) throw new ArgumentException("Webhook secrets must contain at least 32 characters.", nameof(value));
        return new WebhookSecret(value);
    }
}
