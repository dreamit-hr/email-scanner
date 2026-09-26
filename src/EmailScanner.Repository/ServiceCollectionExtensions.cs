using EmailScanner.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailScanner.Repository;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailScannerRepository(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("EmailScanner");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings:EmailScanner must be configured.");
        services.AddDbContext<EmailScannerDbContext>(options => options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IMailboxConnectionRepository, MailboxConnectionRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
