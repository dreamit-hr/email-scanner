using EmailScanner.Domain;
using FluentAssertions;
using Xunit;

namespace EmailScanner.Domain.Tests;

public sealed class MailboxConnectionTests
{
    [Fact]
    public void Connection_owns_mailbox_state_independently_per_tenant()
    {
        var firstTenant = Guid.NewGuid();
        var secondTenant = Guid.NewGuid();
        var first = new MailboxConnection(firstTenant, MailboxProvider.Imap, "billing@example.com", "Billing", "INBOX");
        var second = new MailboxConnection(secondTenant, MailboxProvider.Imap, "billing@example.com", "Billing", "INBOX");

        first.Disable();

        first.Status.Should().Be(MailboxConnectionStatus.Disabled);
        second.Status.Should().Be(MailboxConnectionStatus.Pending);
        first.TenantId.Should().NotBe(second.TenantId);
    }

    [Fact]
    public void EmailAddress_rejects_invalid_addresses()
    {
        var action = () => EmailAddressValue.Create("not-an-address");
        action.Should().Throw<ArgumentException>();
    }
}
