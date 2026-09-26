namespace EmailScanner.Domain.Abstractions;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    public void SetCreatedBy(string? user) => CreatedBy = user;

    protected void Touch(string? user)
    {
        UpdatedUtc = DateTime.UtcNow;
        UpdatedBy = user;
        MarkModified();
    }
}
