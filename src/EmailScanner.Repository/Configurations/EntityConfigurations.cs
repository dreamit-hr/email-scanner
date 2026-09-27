using EmailScanner.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmailScanner.Repository.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants"); builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000); builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.CreatedUtc).HasColumnType("datetime2"); builder.Property(x => x.UpdatedUtc).HasColumnType("datetime2");
        builder.Navigation(x => x.MailboxConnections).HasField("_mailboxes").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.EmailRules).HasField("_rules").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Webhooks).HasField("_webhooks").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State);
    }
}

public sealed class MailboxConnectionConfiguration : IEntityTypeConfiguration<MailboxConnection>
{
    public void Configure(EntityTypeBuilder<MailboxConnection> builder)
    {
        builder.ToTable("MailboxConnections"); builder.HasKey(x => x.Id);
        builder.Property(x => x.EmailAddress).HasMaxLength(320).IsRequired(); builder.Property(x => x.DisplayName).HasMaxLength(256);
        builder.Property(x => x.Folder).HasMaxLength(512).IsRequired(); builder.Property(x => x.SyncToken).HasMaxLength(4000);
        builder.Property(x => x.ImapHost).HasMaxLength(255); builder.Property(x => x.ImapUsername).HasMaxLength(320); builder.Property(x => x.ImapCredentialReference).HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.EmailAddress }); builder.HasIndex(x => new { x.Status, x.LastSyncUtc });
        builder.Property(x => x.CreatedUtc).HasColumnType("datetime2"); builder.Property(x => x.UpdatedUtc).HasColumnType("datetime2");
        builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State);
    }
}

public sealed class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("Emails"); builder.HasKey(x => x.Id); builder.Property(x => x.InternetMessageId).HasMaxLength(998);
        builder.Property(x => x.Sender).HasMaxLength(320); builder.Property(x => x.Subject).HasMaxLength(998);
        builder.Property(x => x.MimeBlobPath).HasMaxLength(1024); builder.Property(x => x.MimeHash).HasMaxLength(128);
        builder.HasIndex(x => new { x.MailboxConnectionId, x.InternetMessageId }).IsUnique(); builder.HasIndex(x => new { x.Status, x.ReceivedUtc });
        builder.Ignore(x => x.BodyPreview); builder.Ignore(x => x.HasAttachments); builder.Ignore(x => x.Tags);
        builder.HasMany(x => x.Recipients).WithOne().HasForeignKey(x => x.EmailId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.EmailId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Recipients).HasField("_recipients").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Attachments).HasField("_attachments").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State);
    }
}

public sealed class MailboxCredentialConfiguration : IEntityTypeConfiguration<MailboxCredential>
{
    public void Configure(EntityTypeBuilder<MailboxCredential> builder) { builder.ToTable("MailboxCredentials"); builder.HasKey(x => x.Id); builder.Property(x => x.CredentialReference).HasMaxLength(512).IsRequired(); builder.Property(x => x.CredentialName).HasMaxLength(200).IsRequired(); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class EmailRecipientConfiguration : IEntityTypeConfiguration<EmailRecipient>
{
    public void Configure(EntityTypeBuilder<EmailRecipient> builder) { builder.ToTable("EmailRecipients"); builder.HasKey(x => x.Id); builder.Property(x => x.Address).HasMaxLength(320).IsRequired(); builder.Property(x => x.DisplayName).HasMaxLength(256); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder) { builder.ToTable("Attachments"); builder.HasKey(x => x.Id); builder.Property(x => x.FileName).HasMaxLength(512).IsRequired(); builder.Property(x => x.ContentType).HasMaxLength(255).IsRequired(); builder.Property(x => x.Hash).HasMaxLength(128).IsRequired(); builder.Property(x => x.BlobPath).HasMaxLength(1024).IsRequired(); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class EmailRuleConfiguration : IEntityTypeConfiguration<EmailRule>
{
    public void Configure(EntityTypeBuilder<EmailRule> builder) { builder.ToTable("EmailRules"); builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(200).IsRequired(); builder.HasIndex(x => new { x.TenantId, x.Priority }); builder.HasMany(x => x.Conditions).WithOne().HasForeignKey(x => x.EmailRuleId).OnDelete(DeleteBehavior.Cascade); builder.HasMany(x => x.Actions).WithOne().HasForeignKey(x => x.EmailRuleId).OnDelete(DeleteBehavior.Cascade); builder.Navigation(x => x.Conditions).HasField("_conditions").UsePropertyAccessMode(PropertyAccessMode.Field); builder.Navigation(x => x.Actions).HasField("_actions").UsePropertyAccessMode(PropertyAccessMode.Field); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class EmailRuleConditionConfiguration : IEntityTypeConfiguration<EmailRuleCondition>
{
    public void Configure(EntityTypeBuilder<EmailRuleCondition> builder) { builder.ToTable("EmailRuleConditions"); builder.HasKey(x => x.Id); builder.Property(x => x.Property).HasMaxLength(128); builder.Property(x => x.Value).HasMaxLength(2048); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class EmailRuleActionConfiguration : IEntityTypeConfiguration<EmailRuleAction>
{
    public void Configure(EntityTypeBuilder<EmailRuleAction> builder) { builder.ToTable("EmailRuleActions"); builder.HasKey(x => x.Id); builder.Property(x => x.ConfigurationJson).HasColumnType("nvarchar(max)"); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class WebhookConfiguration : IEntityTypeConfiguration<Webhook>
{
    public void Configure(EntityTypeBuilder<Webhook> builder) { builder.ToTable("Webhooks"); builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(200).IsRequired(); builder.Property(x => x.Url).HasConversion(x => x.ToString(), x => new Uri(x)).HasMaxLength(2048); builder.Property(x => x.Secret).HasMaxLength(512); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder) { builder.ToTable("WebhookDeliveries"); builder.HasKey(x => x.Id); builder.Property(x => x.ResponseBody).HasMaxLength(4000); builder.HasIndex(x => new { x.WebhookId, x.ExecutedUtc }); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class EmailTagConfiguration : IEntityTypeConfiguration<EmailTag>
{
    public void Configure(EntityTypeBuilder<EmailTag> builder) { builder.ToTable("EmailTags"); builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(100).IsRequired(); builder.Property(x => x.Color).HasMaxLength(32); builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique(); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class EmailTagAssignmentConfiguration : IEntityTypeConfiguration<EmailTagAssignment>
{
    public void Configure(EntityTypeBuilder<EmailTagAssignment> builder) { builder.ToTable("EmailTagAssignments"); builder.HasKey(x => x.Id); builder.HasIndex(x => new { x.EmailId, x.EmailTagId }).IsUnique(); builder.Ignore(x => x.DomainEvents); builder.Ignore(x => x.State); }
}

public sealed class EmailExtractedDocumentConfiguration : IEntityTypeConfiguration<EmailExtractedDocument>
{
    public void Configure(EntityTypeBuilder<EmailExtractedDocument> builder)
    {
        builder.ToTable("EmailExtractedDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.CreatedUtc).HasColumnType("datetime2");
        builder.HasIndex(x => x.EmailId).IsUnique();
        builder.HasOne<Email>().WithMany().HasForeignKey(x => x.EmailId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Entities).WithOne().HasForeignKey(x => x.EmailExtractedDocumentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Entities).HasField("_entities").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class EmailExtractedEntityConfiguration : IEntityTypeConfiguration<EmailExtractedEntity>
{
    public void Configure(EntityTypeBuilder<EmailExtractedEntity> builder)
    {
        builder.ToTable("EmailExtractedEntities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.Property(x => x.Source).HasMaxLength(100).IsRequired();
    }
}

public sealed class WatchedSenderConfiguration : IEntityTypeConfiguration<WatchedSender>
{
    public void Configure(EntityTypeBuilder<WatchedSender> builder)
    {
        builder.ToTable("WatchedSenders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EmailDomain).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.EmailDomain }).IsUnique();
    }
}
