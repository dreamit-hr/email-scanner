namespace EmailScanner.Application.EmailProcessing.Models;

public sealed record EmailExtractedEntityResult(string EntityType, string Value, decimal Confidence, string Source);
