using Azure.Storage.Blobs;
using NodaTime.Extensions;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
public class MediaServices : DbServiceBase<AppDbContext>
{
    private readonly CommonServices _commonServices;

    public MediaServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetMedia(Session session, string container, string fileName)
    {
        return TryGetCommonMedia(container, fileName);
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetCommonMedia(string container, string fileName)
    {
        return TryGetBlobData(container, fileName);
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetUserMedia(Session session, string fileName)
    {
        return TryGetUserMedia(0, fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserMedia(long accountId, string fileName)
    {
        var blobData = await TryGetBlobData(ZExtensions.BlobImages, fileName).ConfigureAwait(false);
        if (blobData == null)
        {
            return null;
        }
     
        return new MediaResult(blobData.LastModified, blobData.ETag, blobData.MediaType, blobData.MediaBytes);
    }

    [ComputeMethod]
    protected virtual async Task<MediaResult> TryGetBlobData(string container, string fileName)
    {
        var blobClient = new BlobClient(_commonServices.Config.BlobStorageConnectionString, container, fileName);
        var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
        if (!blobExists.Value)
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream).ConfigureAwait(false);
        var binaryData = memoryStream.ToArray();

        var properties = await blobClient.GetPropertiesAsync().ConfigureAwait(false);

        return new MediaResult(properties.Value.LastModified.ToInstant(), properties.Value.ETag, "image/*", binaryData);
    }
}