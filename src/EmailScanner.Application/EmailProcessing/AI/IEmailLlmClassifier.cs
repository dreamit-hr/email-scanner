using EmailScanner.Application.EmailProcessing.Models;

namespace EmailScanner.Application.EmailProcessing.AI;

public interface IEmailLlmClassifier
{
    Task<LlmClassificationResponse> ClassifyAsync(EmailClassificationInput input, CancellationToken cancellationToken = default);
}
