using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
await builder.Build().RunAsync();

internal sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Attachment processing worker is waiting for pending attachments.");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
