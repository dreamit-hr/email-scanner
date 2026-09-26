using EmailScanner.Application.MailboxConnections;
using EmailScanner.Application.Rules;
using EmailScanner.Application.MailboxConnections.GetMailboxConnection;
using EmailScanner.Application.MailboxConnections.GetMailboxConnections;
using EmailScanner.Application.MailboxConnections.UpdateMailboxConnection;
using EmailScanner.Application.MailboxConnections.DeleteMailboxConnection;
using EmailScanner.Application.MailboxConnections.EnableMailboxConnection;
using EmailScanner.Application.MailboxConnections.DisableMailboxConnection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EmailScanner.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailScannerApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateMailboxConnectionValidator>();
        services.AddScoped<CreateMailboxConnectionFeature>();
        services.AddScoped<GetMailboxConnectionFeature>();
        services.AddScoped<GetMailboxConnectionsFeature>();
        services.AddScoped<UpdateMailboxConnectionFeature>();
        services.AddScoped<DeleteMailboxConnectionFeature>();
        services.AddScoped<EnableMailboxConnectionFeature>();
        services.AddScoped<DisableMailboxConnectionFeature>();
        services.AddSingleton<RuleEngine>();
        return services;
    }
}
