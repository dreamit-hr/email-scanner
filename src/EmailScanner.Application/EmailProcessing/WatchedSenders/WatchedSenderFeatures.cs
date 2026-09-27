using EmailScanner.Application.Abstractions;
using EmailScanner.Domain;
using EmailScanner.Repository.Abstractions;
using FluentValidation;

namespace EmailScanner.Application.EmailProcessing.WatchedSenders;

public sealed record WatchedSenderResponse(Guid Id, string Name, string EmailDomain, bool IsEnabled)
{
    public static WatchedSenderResponse From(WatchedSender sender) => new(sender.Id, sender.Name, sender.EmailDomain, sender.IsEnabled);
}

public sealed record CreateWatchedSenderRequest(string Name, string EmailDomain);
public sealed record UpdateWatchedSenderRequest(Guid Id, string Name, string EmailDomain, bool IsEnabled);

public sealed class CreateWatchedSenderValidator : AbstractValidator<CreateWatchedSenderRequest>
{
    public CreateWatchedSenderValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmailDomain).NotEmpty().MaximumLength(255).Must(IsValidDomain)
            .WithMessage("EmailDomain must be a valid domain name.");
    }

    internal static bool IsValidDomain(string? value)
    {
        try { _ = WatchedSender.NormalizeDomain(value ?? string.Empty); return true; }
        catch (ArgumentException) { return false; }
    }
}

public sealed class UpdateWatchedSenderValidator : AbstractValidator<UpdateWatchedSenderRequest>
{
    public UpdateWatchedSenderValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmailDomain).NotEmpty().MaximumLength(255).Must(CreateWatchedSenderValidator.IsValidDomain)
            .WithMessage("EmailDomain must be a valid domain name.");
    }
}

public sealed class ListWatchedSendersFeature(IWatchedSenderRepository senders, IClientContext clientContext)
{
    public async Task<FeatureResult<IReadOnlyList<WatchedSenderResponse>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (clientContext.Client is not { } client) return FeatureResult<IReadOnlyList<WatchedSenderResponse>>.Unauthorized();
        var rows = await senders.ListForTenantAsync(client.TenantId, cancellationToken);
        return FeatureResult<IReadOnlyList<WatchedSenderResponse>>.Success(rows.Select(WatchedSenderResponse.From).ToArray());
    }
}

public sealed class GetWatchedSenderFeature(IWatchedSenderRepository senders, IClientContext clientContext)
{
    public async Task<FeatureResult<WatchedSenderResponse>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (clientContext.Client is not { } client) return FeatureResult<WatchedSenderResponse>.Unauthorized();
        var sender = await senders.GetByIdAsync(id, cancellationToken);
        if (sender is null || sender.TenantId != client.TenantId) return FeatureResult<WatchedSenderResponse>.NotFound();
        return FeatureResult<WatchedSenderResponse>.Success(WatchedSenderResponse.From(sender));
    }
}

public sealed class CreateWatchedSenderFeature(
    IValidator<CreateWatchedSenderRequest> validator,
    IWatchedSenderRepository senders,
    IUnitOfWork unitOfWork,
    IClientContext clientContext,
    IPermissionChecker permissionChecker)
{
    public async Task<FeatureResult<WatchedSenderResponse>> ExecuteAsync(CreateWatchedSenderRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return FeatureResult<WatchedSenderResponse>.Validation(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        if (clientContext.Client is not { } client) return FeatureResult<WatchedSenderResponse>.Unauthorized();
        if (!permissionChecker.HasPermission(Permissions.RulesWrite)) return FeatureResult<WatchedSenderResponse>.Forbidden();
        var sender = new WatchedSender(client.TenantId, request.Name, request.EmailDomain);
        senders.Add(sender);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<WatchedSenderResponse>.Success(WatchedSenderResponse.From(sender), 201);
    }
}

public sealed class UpdateWatchedSenderFeature(
    IValidator<UpdateWatchedSenderRequest> validator,
    IWatchedSenderRepository senders,
    IUnitOfWork unitOfWork,
    IClientContext clientContext,
    IPermissionChecker permissionChecker)
{
    public async Task<FeatureResult<WatchedSenderResponse>> ExecuteAsync(UpdateWatchedSenderRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return FeatureResult<WatchedSenderResponse>.Validation(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        if (clientContext.Client is not { } client) return FeatureResult<WatchedSenderResponse>.Unauthorized();
        if (!permissionChecker.HasPermission(Permissions.RulesWrite)) return FeatureResult<WatchedSenderResponse>.Forbidden();
        var sender = await senders.GetByIdAsync(request.Id, cancellationToken);
        if (sender is null || sender.TenantId != client.TenantId) return FeatureResult<WatchedSenderResponse>.NotFound();
        sender.Update(request.Name, request.EmailDomain, request.IsEnabled);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<WatchedSenderResponse>.Success(WatchedSenderResponse.From(sender));
    }
}

public sealed class DeleteWatchedSenderFeature(
    IWatchedSenderRepository senders,
    IUnitOfWork unitOfWork,
    IClientContext clientContext,
    IPermissionChecker permissionChecker)
{
    public async Task<FeatureResult<bool>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (clientContext.Client is not { } client) return FeatureResult<bool>.Unauthorized();
        if (!permissionChecker.HasPermission(Permissions.RulesWrite)) return FeatureResult<bool>.Forbidden();
        var sender = await senders.GetByIdAsync(id, cancellationToken);
        if (sender is null || sender.TenantId != client.TenantId) return FeatureResult<bool>.NotFound();
        senders.Remove(sender);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return FeatureResult<bool>.Success(true);
    }
}
