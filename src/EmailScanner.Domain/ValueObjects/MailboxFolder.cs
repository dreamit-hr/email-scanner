namespace EmailScanner.Domain.ValueObjects;

public sealed record MailboxFolder
{
    private MailboxFolder(string value) => Value = value;
    public string Value { get; }
    public static MailboxFolder Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length > 512 || value.Any(char.IsControl)) throw new ArgumentException("Mailbox folder is invalid.", nameof(value));
        return new MailboxFolder(value.Trim());
    }
}
