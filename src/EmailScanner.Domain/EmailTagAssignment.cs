using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain;

public sealed class EmailTagAssignment : BaseEntity
{
    private EmailTagAssignment() { }
    public EmailTagAssignment(Guid emailId, Guid emailTagId) { EmailId = emailId; EmailTagId = emailTagId; }
    public Guid EmailId { get; private set; }
    public Guid EmailTagId { get; private set; }
}
