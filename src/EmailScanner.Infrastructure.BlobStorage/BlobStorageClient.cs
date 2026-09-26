using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace EmailScanner.Infrastructure.BlobStorage;

public sealed class BlobStorageClient : IBlobStorageClient
{
    private readonly BlobContainerClient _container;

    public BlobStorageClient(IOptions<BlobStorageOptions> options)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ConnectionString)) throw new InvalidOperationException("BlobStorage:ConnectionString must be configured.");
        _container = new BlobServiceClient(settings.ConnectionString).GetBlobContainerClient(settings.ContainerName);
    }

    public async Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        await _container.GetBlobClient(path).UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var response = await _container.GetBlobClient(path).DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteIfExistsAsync(string path, CancellationToken cancellationToken = default) =>
        await _container.GetBlobClient(path).DeleteIfExistsAsync(cancellationToken: cancellationToken);

    public async Task CheckHealthAsync(CancellationToken cancellationToken = default) =>
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
}
