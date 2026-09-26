using EmailScanner.Application.Abstractions;
using EmailScanner.Repository.Abstractions;

namespace EmailScanner.Application.MailboxConnections.DeleteMailboxConnection;

public sealed class DeleteMailboxConnectionFeature(IMailboxConnectionRepository mailboxes, IUnitOfWork unitOfWork, IClientContext clientContext, IPermissionChecker permissionChecker) : IFeature<DeleteMailboxConnectionRequest, bool>
{
    public async Task<FeatureResult<bool>> ExecuteAsync(DeleteMailboxConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var mailbox = await mailboxes.GetByIdAsync(request.Id, cancellationToken);
        if (mailbox is null) return FeatureResult<bool>.NotFound();
        if (clientContext.Client?.TenantId != mailbox.TenantId || !permissionChecker.HasPermission(Permissions.MailboxWrite)) return FeatureResult<bool>.Forbidden();
        mailboxes.Remove(mailbox);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<bool>.Success(true);
    }
}
