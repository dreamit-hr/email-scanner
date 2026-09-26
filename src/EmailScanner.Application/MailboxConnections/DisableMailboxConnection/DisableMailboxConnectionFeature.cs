using EmailScanner.Application.Abstractions;
using EmailScanner.Repository.Abstractions;

namespace EmailScanner.Application.MailboxConnections.DisableMailboxConnection;

public sealed class DisableMailboxConnectionFeature(IMailboxConnectionRepository mailboxes, IUnitOfWork unitOfWork, IClientContext clientContext, IPermissionChecker permissionChecker) : IFeature<DisableMailboxConnectionRequest, bool>
{
    public async Task<FeatureResult<bool>> ExecuteAsync(DisableMailboxConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var mailbox = await mailboxes.GetByIdAsync(request.Id, cancellationToken);
        if (mailbox is null) return FeatureResult<bool>.NotFound();
        if (clientContext.Client?.TenantId != mailbox.TenantId || !permissionChecker.HasPermission(Permissions.MailboxWrite)) return FeatureResult<bool>.Forbidden();
        mailbox.Disable();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<bool>.Success(true);
    }
}
