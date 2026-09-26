using Microsoft.AspNetCore.Mvc;

namespace EmailScanner.Api.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled request exception.");
            if (context.Response.HasStarted) throw;
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "An unexpected error occurred.",
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            }, context.RequestAborted);
        }
    }
}
