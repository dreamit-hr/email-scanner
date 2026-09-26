namespace EmailScanner.Application.Abstractions;

public interface IFeature<in TRequest, TResponse>
{
    Task<FeatureResult<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
