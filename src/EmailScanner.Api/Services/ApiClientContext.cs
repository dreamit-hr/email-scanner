using EmailScanner.Application.Abstractions;

namespace EmailScanner.Api.Services;

public sealed class ApiClientContext(IHttpContextAccessor accessor, IConfiguration configuration) : IClientContext
{
    public ApiClientInfo? Client
    {
        get
        {
            var clientId = accessor.HttpContext?.Request.Headers["X-Client-Id"].ToString();
            var tenantText = clientId is null ? null : configuration[$"ApiClients:{clientId}:TenantId"];
            if (!Guid.TryParse(tenantText, out var tenantId)) return null;
            var permissions = (clientId is null ? null : configuration[$"ApiClients:{clientId}:Permissions"]) ?? string.Empty;
            return new ApiClientInfo(clientId!, tenantId, permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.OrdinalIgnoreCase));
        }
    }
}
