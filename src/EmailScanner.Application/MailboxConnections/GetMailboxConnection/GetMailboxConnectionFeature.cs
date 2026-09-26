using EmailScanner.Application.Abstractions;
using EmailScanner.Application.MailboxConnections;
using EmailScanner.Repository.Abstractions;

namespace EmailScanner.Application.MailboxConnections.GetMailboxConnection;

public sealed class GetMailboxConnectionFeature(IMailboxConnectionRepository mailboxes, IClientContext clientContext) : IFeature<GetMailboxConnectionRequest, MailboxConnectionResponse>
{
    public async Task<FeatureResult<MailboxConnectionResponse>> ExecuteAsync(GetMailboxConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var mailbox = await mailboxes.GetByIdAsync(request.Id, cancellationToken);
        if (mailbox is null) return FeatureResult<MailboxConnectionResponse>.NotFound();
        if (clientContext.Client?.TenantId != mailbox.TenantId) return FeatureResult<MailboxConnectionResponse>.Forbidden();
        return FeatureResult<MailboxConnectionResponse>.Success(MailboxConnectionResponse.From(mailbox));
    }
}
