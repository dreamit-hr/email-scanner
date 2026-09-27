using EmailScanner.Application.Abstractions;
using EmailScanner.Domain;
using EmailScanner.Repository.Abstractions;
using FluentValidation;

namespace EmailScanner.Application.MailboxConnections;

public sealed class CreateMailboxConnectionFeature(
    IValidator<CreateMailboxConnectionRequest> validator,
    ITenantRepository tenants,
    IMailboxConnectionRepository mailboxes,
    IUnitOfWork unitOfWork,
    IClientContext clientContext,
    IPermissionChecker permissionChecker) : IFeature<CreateMailboxConnectionRequest, Guid>
{
    public async Task<FeatureResult<Guid>> ExecuteAsync(CreateMailboxConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return FeatureResult<Guid>.Validation(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        var tenant = await tenants.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null || !tenant.IsEnabled) return FeatureResult<Guid>.NotFound("Enabled tenant was not found.");
        if (clientContext.Client?.TenantId != request.TenantId) return FeatureResult<Guid>.Forbidden();
        if (!permissionChecker.HasPermission(Permissions.MailboxWrite)) return FeatureResult<Guid>.Forbidden();
        var mailbox = new MailboxConnection(request.TenantId, request.Provider, request.EmailAddress, request.DisplayName, request.Folder, request.ImapHost, request.ImapPort, request.ImapUsername, request.ImapCredentialReference);
        try
        {
            mailboxes.Add(mailbox);
        }
        catch (Exception ex)
        {
            var s = ex.Message;
            throw;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<Guid>.Success(mailbox.Id, 201);
    }
}
