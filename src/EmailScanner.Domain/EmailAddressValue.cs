namespace EmailScanner.Domain;

public sealed record EmailAddressValue
{
    private EmailAddressValue(string value) => Value = value;
    public string Value { get; }

    public static EmailAddressValue Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var address = value.Trim();
        if (!System.Net.Mail.MailAddress.TryCreate(address, out var parsed) || !string.Equals(parsed.Address, address, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A valid email address is required.", nameof(value));
        return new EmailAddressValue(address);
    }
}
