using EmailScanner.Application.Abstractions;
using EmailScanner.Repository.Abstractions;

namespace EmailScanner.Application.MailboxConnections.GetMailboxConnections;

public sealed class GetMailboxConnectionsFeature(IMailboxConnectionRepository mailboxes, IClientContext clientContext) : IFeature<GetMailboxConnectionsRequest, IReadOnlyList<MailboxConnectionResponse>>
{
    public async Task<FeatureResult<IReadOnlyList<MailboxConnectionResponse>>> ExecuteAsync(GetMailboxConnectionsRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = clientContext.Client?.TenantId;
        if (tenantId is null) return FeatureResult<IReadOnlyList<MailboxConnectionResponse>>.Unauthorized();
        var rows = await mailboxes.ListAsync(x => x.TenantId == tenantId, cancellationToken);
        return FeatureResult<IReadOnlyList<MailboxConnectionResponse>>.Success(rows.Select(MailboxConnectionResponse.From).ToArray());
    }
}
