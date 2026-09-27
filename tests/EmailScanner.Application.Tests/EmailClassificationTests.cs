using EmailScanner.Application.EmailProcessing;
using EmailScanner.Application.EmailProcessing.AI;
using EmailScanner.Application.EmailProcessing.Classification;
using EmailScanner.Application.EmailProcessing.Models;
using EmailScanner.Domain;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EmailScanner.Application.Tests;

public sealed class EmailClassificationTests
{
    [Theory]
    [InlineData("info@novamed.hr", "novamed.hr", true)]
    [InlineData("user@mail.novamed.hr", "novamed.hr", true)]
    [InlineData("user@othernovamed.hr", "novamed.hr", false)]
    public void Watched_sender_matching_supports_domains_and_subdomains(string sender, string domain, bool expected) =>
        WatchedSenderMatcher.Matches(sender, domain).Should().Be(expected);

    [Theory]
    [InlineData("RTG ortopan", "Orthopantomogram", DocumentCategory.Medical)]
    [InlineData("CBCT cone beam", "CBCT", DocumentCategory.Medical)]
    [InlineData("MR magnetska rezonanca", "MRI", DocumentCategory.Medical)]
    [InlineData("CT kompjuterizirana tomografija", "CT", DocumentCategory.Medical)]
    [InlineData("UZV ultrazvuk", "Ultrasound", DocumentCategory.Medical)]
    [InlineData("nalaz krvi", "BloodTest", DocumentCategory.Medical)]
    [InlineData("ponuda", "Offer", DocumentCategory.Commercial)]
    [InlineData("service contract", "ServiceContract", DocumentCategory.Legal)]
    [InlineData("račun", "Invoice", DocumentCategory.Finance)]
    public void Regex_detects_supported_document_types(string text, string type, DocumentCategory category)
    {
        var detected = new RegexEntityExtractor().DetectDocumentTypes(text);
        detected.Should().Contain((type, category));
    }

    [Theory]
    [InlineData("Poštovani, kod pacijenta MIRO GLAGOLIĆ nalazi se snimka.", "Miro Glagolić")]
    [InlineData("za pacijenta ČEDO NOVAKOVIĆ-PERIĆ", "Čedo Novaković-Perić")]
    public void Patient_regex_supports_Croatian_letters_and_hyphenated_surnames(string text, string expected)
    {
        new RegexEntityExtractor().ExtractPatientNames(text).Should().Contain(expected);
    }

    [Theory]
    [InlineData("Miro_Glagolic_Ortopan.jpg", "Miro Glagolic")]
    [InlineData("Glagolic-Miro-OPG.pdf", "Miro Glagolic")]
    [InlineData("Miro Glagolić CBCT 2026-09-12.png", "Miro Glagolić")]
    public void Filename_parser_extracts_patient_and_exam_date(string fileName, string expectedPatient)
    {
        var entities = new RegexEntityExtractor().ExtractFilenameEntities(fileName);
        entities.Should().Contain(entity => entity.EntityType == "PatientName" && entity.Value == expectedPatient);
        if (fileName.Contains("2026-09-12", StringComparison.Ordinal))
            entities.Should().Contain(entity => entity.EntityType == "ExamDate" && entity.Value == "2026-09-12");
    }

    [Fact]
    public void Quoted_thread_extractor_finds_quoted_lines()
    {
        var extractor = new RegexEntityExtractor();
        const string body = "Poštovani, u prilogu nalaz.\n\n> Molim vas ortopan kod pacijenta Miro Glagolić.";
        var quoted = extractor.ExtractQuotedThread(body);
        quoted.Should().Contain("ortopan kod pacijenta Miro Glagolić");
        extractor.ExtractBodyWithoutQuotedThread(body).Should().Be("Poštovani, u prilogu nalaz.");
    }

    [Fact]
    public async Task Medical_email_without_patient_is_ignored()
    {
        var engine = CreateEngine(enableLlm: false, new FakeLlmClassifier());
        var result = await engine.ClassifyAsync(MakeEmail("RTG ortopan", "U prilogu šaljemo snimku."), [], []);
        result.IsCandidate.Should().BeFalse();
        result.IgnoreReason.Should().Be("PatientNameMissing");
    }

    [Fact]
    public async Task Medical_email_with_patient_is_a_candidate_without_llm()
    {
        var engine = CreateEngine(enableLlm: false, new FakeLlmClassifier());
        var result = await engine.ClassifyAsync(
            MakeEmail("RTG ortopan", "RTG ortopan kod pacijenta MIRO GLAGOLIĆ."), [], []);
        result.IsCandidate.Should().BeTrue();
        result.Category.Should().Be(DocumentCategory.Medical);
        result.Entities.Should().Contain(entity => entity.EntityType == "PatientName" && entity.Value == "Miro Glagolić");
    }

    [Theory]
    [InlineData("Ponuda za uslugu", DocumentCategory.Commercial, "Offer")]
    [InlineData("Ugovor o pružanju usluga", DocumentCategory.Legal, "ServiceContract")]
    [InlineData("Invoice 123", DocumentCategory.Finance, "Invoice")]
    public async Task Commercial_legal_and_finance_documents_are_classified_by_llm_fallback(
        string subject, DocumentCategory category, string type)
    {
        var llm = new FakeLlmClassifier(new LlmClassificationResponse
        {
            Category = category,
            DocumentType = type,
            Confidence = 0.92m,
            Summary = subject
        });
        var result = await CreateEngine(true, llm).ClassifyAsync(MakeEmail(subject, "Molimo pregledajte dokument."), [], []);
        result.IsCandidate.Should().BeTrue();
        result.Category.Should().Be(category);
        result.DocumentType.Should().Be(type);
        llm.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task Llm_fallback_receives_heuristic_context_when_confidence_is_low()
    {
        var llm = new FakeLlmClassifier(new LlmClassificationResponse
        {
            Category = DocumentCategory.Commercial,
            DocumentType = "Offer",
            Confidence = 0.7m,
            Entities = [new("Supplier", "Microsoft Ireland", 0.95m, "LLM")]
        });
        var result = await CreateEngine(true, llm).ClassifyAsync(MakeEmail("Ponuda", "Pregled ponude."), [], []);
        result.IsCandidate.Should().BeTrue();
        result.Entities.Should().Contain(entity => entity.EntityType == "Supplier");
        llm.LastInput.Should().NotBeNull();
        llm.LastInput!.Subject.Should().Be("Ponuda");
        llm.LastInput.HeuristicSignals.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Expected_medical_example_reaches_full_confidence_and_extracts_clinic()
    {
        var sender = new WatchedSender(Guid.NewGuid(), "RTG NovaMed", "@novamed.hr");
        var email = MakeEmail(
            "Re: RTG ortopan",
            "Poštovani, u prilogu šaljemo RTG snimku.\n> Molim vas učinite RTG ortopan snimku kod pacijenta MIRO GLAGOLIĆ.",
            "info@novamed.hr");
        var result = await CreateEngine(false, new FakeLlmClassifier()).ClassifyAsync(
            email,
            [new("Miro_Glagolic_Ortopan.jpg", "image/jpeg")],
            [sender]);
        result.IsCandidate.Should().BeTrue();
        result.Confidence.Should().Be(1m);
        result.Entities.Should().Contain(entity => entity.EntityType == "ClinicName" && entity.Value == "RTG NovaMed");
        result.Entities.Should().Contain(entity => entity.EntityType == "PatientName" && entity.Value == "Miro Glagolić");
    }

    private static IEmailClassificationEngine CreateEngine(bool enableLlm, FakeLlmClassifier classifier) =>
        new EmailClassificationEngine(
            new RegexEntityExtractor(),
            classifier,
            Options.Create(new EmailClassificationOptions { EnableLlmClassification = enableLlm }));

    private static Email MakeEmail(string subject, string body, string sender = "patient@example.com") =>
        new(Guid.NewGuid(), "<message@example.com>", sender, subject, body, DateTime.UtcNow, "emails/test/original.eml", "abc");

    private sealed class FakeLlmClassifier(LlmClassificationResponse? response = null) : IEmailLlmClassifier
    {
        public int CallCount { get; private set; }
        public EmailClassificationInput? LastInput { get; private set; }

        public Task<LlmClassificationResponse> ClassifyAsync(EmailClassificationInput input, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastInput = input;
            return Task.FromResult(response ?? new LlmClassificationResponse());
        }
    }
}
