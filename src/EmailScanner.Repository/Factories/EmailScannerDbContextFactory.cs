using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace EmailScanner.Repository.Factories;

public class EmailScannerDbContextFactory : IDesignTimeDbContextFactory<EmailScannerDbContext>
{
    public EmailScannerDbContext CreateDbContext(string[] args)
    {
        string? connectionString = args.Length != 0 ? args[0] : null;

        var dbOptions = GenerateDbOptions(connectionString);

        return new EmailScannerDbContext(dbOptions);
    }

    private static DbContextOptions<EmailScannerDbContext> GenerateDbOptions(string? connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EmailScannerDbContext>();

        if (string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseSqlServer(o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
        }

        return optionsBuilder.Options;
    }
}
