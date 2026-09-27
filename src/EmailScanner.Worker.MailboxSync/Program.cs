using EmailScanner.Infrastructure.BlobStorage;
using EmailScanner.Infrastructure.ValKey;
using EmailScanner.Repository;
using EmailScanner.Repository.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddEmailScannerRepository(builder.Configuration);
builder.Services.AddEmailScannerBlobStorage(builder.Configuration);
builder.Services.AddEmailScannerValKey(builder.Configuration);
builder.Services.AddScoped<ImapMailboxSyncService>();
builder.Services.AddHostedService<MailboxSyncWorker>();
await builder.Build().RunAsync();

internal sealed class MailboxSyncWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<MailboxSyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = int.TryParse(configuration["MailboxSync:PollIntervalSeconds"], out var configuredInterval)
            ? Math.Clamp(configuredInterval, 5, 3600)
            : 30;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Guid[] mailboxIds;
                await using (var scope = scopeFactory.CreateAsyncScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IMailboxConnectionRepository>();
                    mailboxIds = (await repository.GetEnabledAsync(stoppingToken))
                        .Where(mailbox => mailbox.Provider == EmailScanner.Domain.MailboxProvider.Imap)
                        .Select(mailbox => mailbox.Id)
                        .ToArray();
                }

                foreach (var mailboxId in mailboxIds)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var synchronizer = scope.ServiceProvider.GetRequiredService<ImapMailboxSyncService>();
                    try
                    {
                        await synchronizer.SyncAsync(mailboxId, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "IMAP synchronization failed for mailbox {MailboxId}.", mailboxId);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to enumerate enabled IMAP mailboxes.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}
