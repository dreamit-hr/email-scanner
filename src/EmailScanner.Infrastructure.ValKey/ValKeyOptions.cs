namespace EmailScanner.Infrastructure.ValKey;

public sealed class ValKeyOptions
{
    public const string SectionName = "ValKey";
    public string ConnectionString { get; init; } = "localhost:6379,abortConnect=false";
    public string InstanceName { get; init; } = "emailscanner:";
}
