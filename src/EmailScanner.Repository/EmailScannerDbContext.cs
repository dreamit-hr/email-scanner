using EmailScanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmailScanner.Repository;

public sealed class EmailScannerDbContext(DbContextOptions<EmailScannerDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenant => Set<Tenant>();
    public DbSet<MailboxConnection> MailboxConnection => Set<MailboxConnection>();
    public DbSet<MailboxCredential> MailboxCredential => Set<MailboxCredential>();
    public DbSet<Email> Email => Set<Email>();
    public DbSet<EmailRecipient> EmailRecipient => Set<EmailRecipient>();
    public DbSet<Attachment> Attachment => Set<Attachment>();
    public DbSet<EmailRule> EmailRule => Set<EmailRule>();
    public DbSet<EmailRuleCondition> EmailRuleCondition => Set<EmailRuleCondition>();
    public DbSet<EmailRuleAction> EmailRuleAction => Set<EmailRuleAction>();
    public DbSet<Webhook> Webhook => Set<Webhook>();
    public DbSet<WebhookDelivery> WebhookDelivery => Set<WebhookDelivery>();
    public DbSet<EmailTag> EmailTag => Set<EmailTag>();
    public DbSet<EmailTagAssignment> EmailTagAssignment => Set<EmailTagAssignment>();
    public DbSet<EmailExtractedDocument> EmailExtractedDocument => Set<EmailExtractedDocument>();
    public DbSet<EmailExtractedEntity> EmailExtractedEntity => Set<EmailExtractedEntity>();
    public DbSet<WatchedSender> WatchedSender => Set<WatchedSender>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmailScannerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<EmailScanner.Domain.Abstractions.AuditableEntity>())
        {
            if (entry.State == EntityState.Modified) entry.Property(nameof(EmailScanner.Domain.Abstractions.AuditableEntity.UpdatedUtc)).CurrentValue = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
