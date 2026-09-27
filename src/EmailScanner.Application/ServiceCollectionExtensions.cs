using EmailScanner.Application.MailboxConnections;
using EmailScanner.Application.Rules;
using EmailScanner.Application.EmailProcessing;
using EmailScanner.Application.EmailProcessing.AI;
using EmailScanner.Application.EmailProcessing.Classification;
using EmailScanner.Application.EmailProcessing.WatchedSenders;
using EmailScanner.Application.MailboxConnections.GetMailboxConnection;
using EmailScanner.Application.MailboxConnections.GetMailboxConnections;
using EmailScanner.Application.MailboxConnections.UpdateMailboxConnection;
using EmailScanner.Application.MailboxConnections.DeleteMailboxConnection;
using EmailScanner.Application.MailboxConnections.EnableMailboxConnection;
using EmailScanner.Application.MailboxConnections.DisableMailboxConnection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EmailScanner.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailScannerApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<CreateMailboxConnectionValidator>();
        services.Configure<EmailClassificationOptions>(configuration.GetSection(EmailClassificationOptions.SectionName));
        services.Configure<AzureOpenAiOptions>(configuration.GetSection(AzureOpenAiOptions.SectionName));
        services.AddScoped<CreateMailboxConnectionFeature>();
        services.AddScoped<GetMailboxConnectionFeature>();
        services.AddScoped<GetMailboxConnectionsFeature>();
        services.AddScoped<UpdateMailboxConnectionFeature>();
        services.AddScoped<DeleteMailboxConnectionFeature>();
        services.AddScoped<EnableMailboxConnectionFeature>();
        services.AddScoped<DisableMailboxConnectionFeature>();
        services.AddScoped<ListWatchedSendersFeature>();
        services.AddScoped<GetWatchedSenderFeature>();
        services.AddScoped<CreateWatchedSenderFeature>();
        services.AddScoped<UpdateWatchedSenderFeature>();
        services.AddScoped<DeleteWatchedSenderFeature>();
        services.AddSingleton<RuleEngine>();
        services.AddSingleton<RegexEntityExtractor>();
        services.AddSingleton<IEmailLlmClassifier, AzureOpenAiEmailLlmClassifier>();
        services.AddScoped<IEmailClassificationEngine, EmailClassificationEngine>();
        return services;
    }
}
