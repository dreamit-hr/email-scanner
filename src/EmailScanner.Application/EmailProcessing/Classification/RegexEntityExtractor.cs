using System.Globalization;
using System.Text.RegularExpressions;
using EmailScanner.Application.EmailProcessing.Models;
using EmailScanner.Domain;

namespace EmailScanner.Application.EmailProcessing.Classification;

public sealed class RegexEntityExtractor
{
    private static readonly (string Type, DocumentCategory Category, Regex Pattern)[] DocumentPatterns =
    [
        ("Orthopantomogram", DocumentCategory.Medical, Pattern(@"orthopantomogram|orthopantomografija|rtg\s*ortopan|ortopan|orthopan|\bopg\b")),
        ("CBCT", DocumentCategory.Medical, Pattern(@"\bcbct\b|cone\s*beam")),
        ("MRI", DocumentCategory.Medical, Pattern(@"\bmri\b|\bmr\b|magnetska\s+rezonanc")),
        ("CT", DocumentCategory.Medical, Pattern(@"\bct\b|kompjuterizirana\s+tomograf")),
        ("Ultrasound", DocumentCategory.Medical, Pattern(@"\buzv\b|ultrazvuk")),
        ("BloodTest", DocumentCategory.Medical, Pattern(@"krvna\s+slika|nalaz\s+krvi|laboratorijski\s+nalaz")),
        ("Invoice", DocumentCategory.Finance, Pattern(@"\binvoice\b|\bfaktura\b|\bračun\b|\bracun\b")),
        ("Offer", DocumentCategory.Commercial, Pattern(@"\boffer\b|\bquotation\b|\bponuda\b")),
        ("ServiceContract", DocumentCategory.Legal, Pattern(@"\bservice\s+contract\b|\bcontract\b|\bugovor\b"))
    ];

    private static readonly Regex PatientPattern = Pattern(
        @"(?:kod\s+pacijenta|za\s+pacijenta|pacijent(?:a)?)\s*[:\-]?\s*(?<name>[\p{Lu}][\p{L}\p{M}]+(?:[-'][\p{Lu}][\p{L}\p{M}]+)?\s+[\p{Lu}][\p{L}\p{M}]+(?:[-'][\p{Lu}][\p{L}\p{M}]+)?)");
    private static readonly Regex ExamDatePattern = Pattern(@"\b(?<date>\d{4}-\d{2}-\d{2})\b");
    private static readonly Regex FilenameSeparatorPattern = new(@"[\s_-]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly CultureInfo CroatianCulture = CultureInfo.GetCultureInfo("hr-HR");

    public IReadOnlyList<(string Type, DocumentCategory Category)> DetectDocumentTypes(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        return DocumentPatterns
            .Where(item => item.Pattern.IsMatch(text))
            .Select(item => (item.Type, item.Category))
            .ToArray();
    }

    public IReadOnlyList<string> ExtractPatientNames(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : PatientPattern.Matches(text)
                .Select(match => NormalizeName(match.Groups["name"].Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

    public string ExtractQuotedThread(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;
        var lines = body.Replace("\r\n", "\n").Split('\n');
        var start = FindQuotedStart(lines);

        if (start < 0) return string.Empty;
        return string.Join('\n', lines.Skip(start).Select(line => Regex.Replace(line.TrimStart(), @"^>\s?", string.Empty)));
    }

    public string ExtractBodyWithoutQuotedThread(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;
        var lines = body.Replace("\r\n", "\n").Split('\n');
        var start = FindQuotedStart(lines);
        return start < 0 ? body : string.Join('\n', lines.Take(start)).TrimEnd();
    }

    public IReadOnlyList<EmailExtractedEntityResult> ExtractFilenameEntities(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return [];
        var entities = new List<EmailExtractedEntityResult>();
        var baseName = Path.GetFileNameWithoutExtension(fileName);

        var dateMatch = ExamDatePattern.Match(baseName);
        if (dateMatch.Success && DateOnly.TryParseExact(dateMatch.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var examDate))
            entities.Add(new("ExamDate", examDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), 0.95m, "AttachmentFilename"));

        var documentMatch = DocumentPatterns.FirstOrDefault(item => item.Pattern.IsMatch(baseName));
        if (documentMatch.Pattern is null) return entities;
        if (documentMatch.Category != DocumentCategory.Medical) return entities;

        var documentIndex = documentMatch.Pattern.Match(baseName).Index;
        var patientText = baseName[..documentIndex];
        if (dateMatch.Success) patientText = patientText.Replace(dateMatch.Value, string.Empty, StringComparison.Ordinal);
        var nameParts = FilenameSeparatorPattern.Split(patientText.Trim('_', '-', ' ', '.'))
            .Where(part => part.Length > 1 && !Regex.IsMatch(part, @"^\d+$"))
            .ToArray();

        if (nameParts.Length >= 2)
        {
            var first = nameParts[0];
            var second = nameParts[1];
            var name = first.Length > second.Length ? $"{second} {first}" : $"{first} {second}";
            entities.Add(new("PatientName", NormalizeName(name), 0.9m, "AttachmentFilename"));
        }

        return entities;
    }

    private static string NormalizeName(string value)
    {
        var normalized = Regex.Replace(value.Trim(), @"\s+", " ").ToLower(CroatianCulture);
        return CroatianCulture.TextInfo.ToTitleCase(normalized);
    }

    private static int FindQuotedStart(string[] lines) => Array.FindIndex(lines, line =>
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith('>')
            || trimmed.Contains("-----Original Message-----", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("From:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("Od:", StringComparison.OrdinalIgnoreCase)
            || Regex.IsMatch(trimmed, @"^(?:on|dana\b).{0,300}(?:wrote:|napisao|napisala):?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    });

    private static Regex Pattern(string pattern) =>
        new(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
}
