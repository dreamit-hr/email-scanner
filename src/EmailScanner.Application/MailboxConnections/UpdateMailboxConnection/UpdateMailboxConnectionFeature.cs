using EmailScanner.Application.Abstractions;
using EmailScanner.Repository.Abstractions;
using FluentValidation;

namespace EmailScanner.Application.MailboxConnections.UpdateMailboxConnection;

public sealed class UpdateMailboxConnectionFeature(IValidator<UpdateMailboxConnectionRequest> validator, IMailboxConnectionRepository mailboxes, IUnitOfWork unitOfWork, IClientContext clientContext, IPermissionChecker permissionChecker) : IFeature<UpdateMailboxConnectionRequest, bool>
{
    public async Task<FeatureResult<bool>> ExecuteAsync(UpdateMailboxConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return FeatureResult<bool>.Validation(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        var mailbox = await mailboxes.GetByIdAsync(request.Id, cancellationToken);
        if (mailbox is null) return FeatureResult<bool>.NotFound();
        if (clientContext.Client?.TenantId != mailbox.TenantId || !permissionChecker.HasPermission(Permissions.MailboxWrite)) return FeatureResult<bool>.Forbidden();
        mailbox.UpdateDetails(request.DisplayName, request.SyncMode);
        mailbox.UpdateFolder(request.Folder);
        mailbox.UpdateImapSettings(
            request.ImapHost ?? mailbox.ImapHost,
            request.ImapPort ?? mailbox.ImapPort,
            request.ImapUsername ?? mailbox.ImapUsername,
            request.ImapCredentialReference ?? mailbox.ImapCredentialReference);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<bool>.Success(true);
    }
}
