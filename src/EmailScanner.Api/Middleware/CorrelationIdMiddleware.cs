using EmailScanner.Application.Abstractions;
using Serilog.Context;

namespace EmailScanner.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requested = context.Request.Headers[HeaderNames.CorrelationId].ToString();
        var correlationId = Guid.TryParse(requested, out _) && requested.Length <= 64 ? requested : Guid.NewGuid().ToString("N");
        context.Response.Headers[HeaderNames.CorrelationId] = correlationId;
        using (LogContext.PushProperty("CorrelationId", correlationId)) await next(context);
    }
}
