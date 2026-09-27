using EmailScanner.Api;
using EmailScanner.Application;
using EmailScanner.Repository;
using EmailScanner.Api.Filters;
using EmailScanner.Api.Middleware;
using EmailScanner.Api.Health;
using EmailScanner.Application.Abstractions;
using EmailScanner.Api.Services;
using EmailScanner.Infrastructure.ValKey;
using EmailScanner.Infrastructure.BlobStorage;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo.Console().WriteTo.File("logs/emailscanner-.log", rollingInterval: RollingInterval.Day));
builder.Services.AddControllers(options => options.Filters.Add<ApiResponseFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ClientId", new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey, Name = "X-Client-Id", In = Microsoft.OpenApi.Models.ParameterLocation.Header });
    options.AddSecurityDefinition("ClientSecret", new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey, Name = "X-Client-Secret", In = Microsoft.OpenApi.Models.ParameterLocation.Header });
    options.AddSecurityDefinition("CorrelationId", new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey, Name = "X-Correlation-Id", In = Microsoft.OpenApi.Models.ParameterLocation.Header });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "ClientId" } }] = [],
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "ClientSecret" } }] = [],
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "CorrelationId" } }] = []
    });
});
builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("EmailScanner") ?? string.Empty).AddCheck<ValKeyHealthCheck>("valkey").AddCheck<BlobStorageHealthCheck>("blob-storage");
builder.Services.AddEmailScannerApplication();
builder.Services.AddEmailScannerRepository(builder.Configuration);
builder.Services.AddSingleton<ApiKeyValidator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClientContext, ApiClientContext>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();
builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();
builder.Services.AddEmailScannerValKey(builder.Configuration);
builder.Services.AddEmailScannerBlobStorage(builder.Configuration);

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EmailScannerDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
