using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class EmailRecipient : BaseEntity
{
    private EmailRecipient() { }
    public EmailRecipient(Guid emailId, RecipientType type, string address, string? displayName)
    {
        EmailId = emailId; RecipientType = type; Address = EmailAddressValue.Create(address).Value; DisplayName = displayName?.Trim();
    }
    public Guid EmailId { get; private set; }
    public RecipientType RecipientType { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
}
