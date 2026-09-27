namespace EmailScanner.Application.EmailProcessing;

public sealed class EmailClassificationOptions
{
    public const string SectionName = "EmailClassification";
    public bool EnableLlmClassification { get; set; }
    public decimal PersistThreshold { get; set; } = 90m;
    public decimal LlmThreshold { get; set; } = 60m;
    public bool MedicalRequiresPatient { get; set; } = true;
}
