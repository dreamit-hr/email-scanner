namespace EmailScanner.Application.EmailProcessing.Models;

public sealed record EmailClassificationSignal(string Source, string Type, string Value, int Score);
