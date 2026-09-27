using EmailScanner.Application;
using EmailScanner.Application.Abstractions;
using EmailScanner.Application.EmailProcessing;
using EmailScanner.Application.EmailProcessing.Models;
using EmailScanner.Domain;
using EmailScanner.Infrastructure.ValKey;
using EmailScanner.Repository;
using EmailScanner.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());
builder.Services.AddEmailScannerApplication(builder.Configuration);
builder.Services.AddEmailScannerRepository(builder.Configuration);
builder.Services.AddEmailScannerValKey(builder.Configuration);
builder.Services.AddScoped<QueuedEmailProcessor>();
builder.Services.AddHostedService<Worker>();
await builder.Build().RunAsync();

internal sealed class Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Guid[] queuedIds;
                await using (var scope = scopeFactory.CreateAsyncScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<EmailScannerDbContext>();
                    queuedIds = await context.Email.AsNoTracking()
                        .Where(email => email.Status == EmailStatus.Queued)
                        .OrderBy(email => email.ReceivedUtc)
                        .Select(email => email.Id)
                        .Take(25)
                        .ToArrayAsync(stoppingToken);
                }

                foreach (var emailId in queuedIds)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var processor = scope.ServiceProvider.GetRequiredService<QueuedEmailProcessor>();
                    try
                    {
                        await processor.ProcessAsync(emailId, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Email classification failed for email {EmailId}.", emailId);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to load queued email messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}

internal sealed class QueuedEmailProcessor(
    EmailScannerDbContext dbContext,
    IWatchedSenderRepository watchedSenders,
    IEmailClassificationEngine classificationEngine,
    IDistributedLockService distributedLock,
    ILogger<QueuedEmailProcessor> logger)
{
    public async Task ProcessAsync(Guid emailId, CancellationToken cancellationToken)
    {
        await using var lease = await distributedLock.TryAcquireAsync(
            $"email-processing:{emailId:D}", TimeSpan.FromMinutes(10), cancellationToken);
        if (lease is null) return;

        var email = await dbContext.Email
            .Include(x => x.Attachments)
            .SingleOrDefaultAsync(x => x.Id == emailId, cancellationToken);
        if (email is null || email.Status != EmailStatus.Queued) return;

        var existingDocument = await dbContext.EmailExtractedDocument
            .AnyAsync(document => document.EmailId == email.Id, cancellationToken);
        if (existingDocument)
        {
            email.MarkProcessed();
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var tenantId = await dbContext.MailboxConnection.AsNoTracking()
                .Where(mailbox => mailbox.Id == email.MailboxConnectionId)
                .Select(mailbox => mailbox.TenantId)
                .SingleAsync(cancellationToken);
        var rules = await watchedSenders.GetEnabledForTenantAsync(tenantId, cancellationToken);

        var attachmentMetadata = email.Attachments
            .Select(attachment => new EmailAttachmentMetadata(attachment.FileName, attachment.ContentType))
            .ToArray();
        EmailClassificationResult classification;
        try
        {
            classification = await classificationEngine.ClassifyAsync(
                email, attachmentMetadata, rules, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            email.MarkFailed();
            await dbContext.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        logger.LogInformation(
            "Email classification completed. EmailId={EmailId} Sender={Sender} Category={Category} DocumentType={DocumentType} Confidence={Confidence} IgnoreReason={IgnoreReason} Signals={@Signals}",
            email.Id,
            email.Sender,
            classification.Category,
            classification.DocumentType,
            classification.Confidence,
            classification.IgnoreReason,
            classification.Signals);

        if (classification.IsCandidate)
        {
            var document = new EmailExtractedDocument(
                tenantId,
                email.Id,
                classification.Category,
                classification.DocumentType,
                classification.Confidence,
                classification.Summary);
            foreach (var entity in classification.Entities)
                document.AddEntity(entity.EntityType, entity.Value, entity.Confidence, entity.Source);
            dbContext.EmailExtractedDocument.Add(document);
        }

        email.MarkProcessed();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
