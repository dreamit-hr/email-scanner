using EmailScanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmailScanner.Repository;

public sealed class EmailScannerDbContext(DbContextOptions<EmailScannerDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<MailboxConnection> MailboxConnections => Set<MailboxConnection>();
    public DbSet<MailboxCredential> MailboxCredentials => Set<MailboxCredential>();
    public DbSet<Email> Emails => Set<Email>();
    public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<EmailRule> EmailRules => Set<EmailRule>();
    public DbSet<EmailRuleCondition> EmailRuleConditions => Set<EmailRuleCondition>();
    public DbSet<EmailRuleAction> EmailRuleActions => Set<EmailRuleAction>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<EmailTag> EmailTags => Set<EmailTag>();
    public DbSet<EmailTagAssignment> EmailTagAssignments => Set<EmailTagAssignment>();

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
