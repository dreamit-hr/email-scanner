namespace EmailScanner.Application.EmailProcessing.AI;

public sealed class AzureOpenAiOptions
{
    public const string SectionName = "AzureOpenAI";
    public string Endpoint { get; set; } = string.Empty;
    public string Deployment { get; set; } = "gpt-5-mini";
    public string ApiKey { get; set; } = string.Empty;
}
