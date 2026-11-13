using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services;

public sealed class AzureBlobStorageService : IBlobStorageService
{
    private readonly IConfiguration _configuration;
    private BlobContainerClient? _container;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private BlobContainerClient GetContainer()
    {
        if (_container is not null)
            return _container;

        var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
        var containerName = _configuration["AzureBlobStorage:ContainerName"] ?? "event-banners";
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure AzureBlobStorage:ConnectionString before uploading event banners.");

        _container = new BlobServiceClient(connectionString).GetBlobContainerClient(containerName);
        return _container;
    }

    public async Task<string> UploadEventBannerAsync(Stream content, string contentType, string fileExtension, CancellationToken cancellationToken = default)
    {
        var container = GetContainer();
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        var blobName = $"{Guid.NewGuid():N}{fileExtension}";
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);
        return blob.Uri.ToString();
    }

    public async Task DeleteByUrlAsync(string? blobUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobUrl) || !Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri))
            return;

        // Only delete blobs belonging to the configured container/account.
        var container = GetContainer();
        var containerUri = container.Uri;
        if (!string.Equals(uri.Host, containerUri.Host, StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.StartsWith(containerUri.AbsolutePath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
            return;

        var blobName = Uri.UnescapeDataString(uri.AbsolutePath[(containerUri.AbsolutePath.TrimEnd('/').Length + 1)..]);
        await container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
