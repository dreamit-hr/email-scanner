using EmailScanner.Application.Abstractions;
using EmailScanner.Repository.Abstractions;

namespace EmailScanner.Application.MailboxConnections.EnableMailboxConnection;

public sealed class EnableMailboxConnectionFeature(IMailboxConnectionRepository mailboxes, IUnitOfWork unitOfWork, IClientContext clientContext, IPermissionChecker permissionChecker) : IFeature<EnableMailboxConnectionRequest, bool>
{
    public async Task<FeatureResult<bool>> ExecuteAsync(EnableMailboxConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var mailbox = await mailboxes.GetByIdAsync(request.Id, cancellationToken);
        if (mailbox is null) return FeatureResult<bool>.NotFound();
        if (clientContext.Client?.TenantId != mailbox.TenantId || !permissionChecker.HasPermission(Permissions.MailboxWrite)) return FeatureResult<bool>.Forbidden();
        mailbox.Enable();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<bool>.Success(true);
    }
}
