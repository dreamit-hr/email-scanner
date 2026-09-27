using EmailScanner.Application.EmailProcessing.AI;
using EmailScanner.Application.EmailProcessing.Models;
using EmailScanner.Domain;
using Microsoft.Extensions.Options;

namespace EmailScanner.Application.EmailProcessing.Classification;

public sealed class EmailClassificationEngine(
    RegexEntityExtractor extractor,
    IEmailLlmClassifier llmClassifier,
    IOptions<EmailClassificationOptions> options) : IEmailClassificationEngine
{
    private readonly EmailClassificationOptions _options = options.Value;

    public async Task<EmailClassificationResult> ClassifyAsync(
        Email email,
        IReadOnlyList<EmailAttachmentMetadata> attachments,
        IReadOnlyList<WatchedSender> watchedSenders,
        string? ocrText = null,
        CancellationToken cancellationToken = default)
    {
        var signals = new List<EmailClassificationSignal>();
        var entities = new List<EmailExtractedEntityResult>();
        DocumentCategory category = DocumentCategory.Unknown;
        string? documentType = null;

        foreach (var sender in watchedSenders.Where(sender => sender.IsEnabled && WatchedSenderMatcher.Matches(email.Sender, sender.EmailDomain)))
        {
            signals.Add(new("Sender", "SenderMatch", sender.EmailDomain, 20));
            entities.Add(new("ClinicName", sender.Name, 0.95m, "Sender"));
        }

        var quotedThread = extractor.ExtractQuotedThread(email.Body);
        var bodyWithoutQuotedThread = extractor.ExtractBodyWithoutQuotedThread(email.Body);
        AddDocumentSignals(email.Subject, "Subject", 30);
        AddDocumentSignals(bodyWithoutQuotedThread, "Body", 30);
        AddDocumentSignals(quotedThread, "QuotedThread", 15);
        AddDocumentSignals(ocrText ?? string.Empty, "OCR", 30);

        AddPatientSignals(bodyWithoutQuotedThread, "Body", 30);
        AddPatientSignals(quotedThread, "QuotedThread", 15);
        AddPatientSignals(ocrText ?? string.Empty, "OCR", 30);

        foreach (var attachment in attachments)
        {
            var document = extractor.DetectDocumentTypes(attachment.FileName).FirstOrDefault();
            if (!string.IsNullOrEmpty(document.Type))
            {
                signals.Add(new("AttachmentFilename", "DocumentType", document.Type, 20));
                SelectDocument(document.Type, document.Category);
            }

            foreach (var entity in extractor.ExtractFilenameEntities(attachment.FileName))
            {
                entities.Add(entity);
                if (entity.EntityType == "PatientName")
                    signals.Add(new("AttachmentFilename", entity.EntityType, entity.Value, 25));
            }
        }

        var heuristicConfidence = Math.Min(100, signals.Sum(signal => signal.Score)) / 100m;
        var llmWasUsed = false;
        var llmQualified = false;
        decimal llmConfidence = 0m;
        var summary = BuildSummary(email);
        var needsMedicalPatient = category == DocumentCategory.Medical
            && _options.MedicalRequiresPatient
            && !entities.Any(entity => entity.EntityType.Equals("PatientName", StringComparison.OrdinalIgnoreCase));

        var persistThreshold = _options.PersistThreshold / 100m;
        var llmThreshold = _options.LlmThreshold / 100m;
        if (_options.EnableLlmClassification && (heuristicConfidence < persistThreshold || needsMedicalPatient || category == DocumentCategory.Unknown))
        {
            var response = await llmClassifier.ClassifyAsync(new EmailClassificationInput
            {
                Sender = email.Sender,
                Subject = email.Subject,
                Body = bodyWithoutQuotedThread,
                QuotedThread = quotedThread,
                Attachments = attachments,
                HeuristicSignals = signals,
                OcrText = ocrText
            }, cancellationToken);

            llmWasUsed = true;
            if (response.Category != DocumentCategory.Unknown) category = response.Category;
            if (!string.IsNullOrWhiteSpace(response.DocumentType)) documentType = response.DocumentType.Trim();
            if (!string.IsNullOrWhiteSpace(response.Summary)) summary = response.Summary.Trim();
            entities.AddRange(response.Entities.Where(entity => !string.IsNullOrWhiteSpace(entity.EntityType) && !string.IsNullOrWhiteSpace(entity.Value)));
            llmConfidence = Math.Clamp(response.Confidence, 0m, 1m);
            llmQualified = response.Category != DocumentCategory.Unknown && llmConfidence >= llmThreshold;
            heuristicConfidence = Math.Max(heuristicConfidence, llmConfidence);
            signals.Add(new("LLM", "DocumentType", documentType ?? string.Empty, (int)(llmConfidence * 100m)));
            foreach (var entity in response.Entities)
                signals.Add(new("LLM", entity.EntityType, entity.Value, (int)(Math.Clamp(entity.Confidence, 0m, 1m) * 100m)));
        }

        documentType ??= GetDocumentTypeForCategory(category);
        entities = entities
            .Where(entity => !string.IsNullOrWhiteSpace(entity.EntityType) && !string.IsNullOrWhiteSpace(entity.Value))
            .GroupBy(entity => (entity.EntityType.Trim(), entity.Value.Trim()), StringTupleComparer.Instance)
            .Select(group => group.OrderByDescending(entity => entity.Confidence).First())
            .ToList();

        var hasMedicalPatient = entities.Any(entity => entity.EntityType.Equals("PatientName", StringComparison.OrdinalIgnoreCase));
        var candidate = category != DocumentCategory.Unknown
            && !string.IsNullOrWhiteSpace(documentType)
            && (llmWasUsed ? llmQualified : heuristicConfidence >= persistThreshold);
        string? ignoreReason = null;
        if (category == DocumentCategory.Medical && _options.MedicalRequiresPatient && !hasMedicalPatient)
        {
            candidate = false;
            ignoreReason = "PatientNameMissing";
        }
        else if (!candidate)
        {
            ignoreReason = category == DocumentCategory.Unknown ? "CategoryUnknown" : "ConfidenceBelowThreshold";
        }

        return new EmailClassificationResult
        {
            IsCandidate = candidate,
            Category = category,
            DocumentType = documentType ?? string.Empty,
            Confidence = heuristicConfidence,
            Summary = summary,
            IgnoreReason = ignoreReason,
            Signals = signals,
            Entities = entities
        };

        void AddDocumentSignals(string text, string source, int score)
        {
            foreach (var document in extractor.DetectDocumentTypes(text))
            {
                signals.Add(new(source, "DocumentType", document.Type, score));
                SelectDocument(document.Type, document.Category);
            }
        }

        void AddPatientSignals(string text, string source, int score)
        {
            foreach (var patientName in extractor.ExtractPatientNames(text))
            {
                signals.Add(new(source, "PatientName", patientName, score));
                entities.Add(new("PatientName", patientName, Math.Min(score, 30) / 30m, source));
            }
        }

        void SelectDocument(string type, DocumentCategory selectedCategory)
        {
            if (documentType is null || category == DocumentCategory.Unknown || category == DocumentCategory.Medical && selectedCategory != DocumentCategory.Medical)
            {
                documentType = type;
                category = selectedCategory;
            }
        }
    }

    private static string BuildSummary(Email email)
    {
        var summary = string.IsNullOrWhiteSpace(email.Subject) ? email.Body : email.Subject;
        summary = string.Join(' ', summary.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return summary.Length <= 1000 ? summary : summary[..1000];
    }

    private static string GetDocumentTypeForCategory(DocumentCategory category) => category switch
    {
        DocumentCategory.Medical => "MedicalDocument",
        DocumentCategory.Commercial => "CommercialDocument",
        DocumentCategory.Legal => "LegalDocument",
        DocumentCategory.Finance => "FinancialDocument",
        _ => string.Empty
    };

    private sealed class StringTupleComparer : IEqualityComparer<(string, string)>
    {
        public static readonly StringTupleComparer Instance = new();
        public bool Equals((string, string) x, (string, string) y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x.Item1, y.Item1) && StringComparer.OrdinalIgnoreCase.Equals(x.Item2, y.Item2);
        public int GetHashCode((string, string) value) => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(value.Item1),
            StringComparer.OrdinalIgnoreCase.GetHashCode(value.Item2));
    }
}
