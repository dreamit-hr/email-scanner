using EmailScanner.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmailScanner.Repository.Tests;

public sealed class EmailScannerModelTests
{
    [Fact]
    public void Model_contains_all_persisted_aggregates()
    {
        var options = new DbContextOptionsBuilder<EmailScannerDbContext>().UseSqlServer("Server=localhost;Database=EmailScanner;User Id=sa;Password=NotARealPassword123!;TrustServerCertificate=True").Options;
        using var context = new EmailScannerDbContext(options);
        var names = context.Model.GetEntityTypes().Select(x => x.GetTableName()).ToHashSet();
        names.Should().Contain(["Tenants", "MailboxConnections", "Emails", "Attachments", "EmailRules", "Webhooks", "WebhookDeliveries", "EmailTags"]);
    }
}
