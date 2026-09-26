using EmailScanner.Application.Abstractions;

namespace EmailScanner.Api.Services;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId => accessor.HttpContext?.User.Identity?.Name;
}

public sealed class PermissionChecker(IClientContext clientContext) : IPermissionChecker
{
    public bool HasPermission(string permission) => clientContext.Client?.Permissions.Contains(permission) == true;
}

public sealed class CorrelationContext(IHttpContextAccessor accessor) : ICorrelationContext
{
    public string CorrelationId => accessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
}
