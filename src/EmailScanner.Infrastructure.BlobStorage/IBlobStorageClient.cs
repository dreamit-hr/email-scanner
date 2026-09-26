namespace EmailScanner.Infrastructure.BlobStorage;

public interface IBlobStorageClient
{
    Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteIfExistsAsync(string path, CancellationToken cancellationToken = default);
    Task CheckHealthAsync(CancellationToken cancellationToken = default);
}
