namespace EmailScanner.Api;

public sealed class ApiKeyValidator(IConfiguration configuration)
{
    public bool IsValid(string? clientId, string? clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret)) return false;
        var configured = configuration[$"ApiClients:{clientId}:Secret"];
        return configured is not null && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(configured), System.Text.Encoding.UTF8.GetBytes(clientSecret));
    }
}

public sealed class ApiKeyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApiKeyValidator validator)
    {
        if (context.Request.Path.StartsWithSegments("/health") || context.Request.Path == "/swagger")
        {
            await next(context);
            return;
        }

        if (!validator.IsValid(context.Request.Headers["X-Client-Id"], context.Request.Headers["X-Client-Secret"]))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "invalid_client", message = "Valid client credentials are required." });
            return;
        }

        await next(context);
    }
}
