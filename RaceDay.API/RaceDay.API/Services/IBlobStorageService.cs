namespace RaceDay.API.Services;

public interface IBlobStorageService
{
    Task<string> UploadEventBannerAsync(Stream content, string contentType, string fileExtension, CancellationToken cancellationToken = default);
    Task DeleteByUrlAsync(string? blobUrl, CancellationToken cancellationToken = default);
}
