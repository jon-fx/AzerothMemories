using Azure.Storage.Blobs;

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
        return TryGetMedia(container, fileName);
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetImage(Session session, string fileName)
    {
        return TryGetMedia(ZExtensions.BlobImages, fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetMedia(string container, string fileName)
    {
        var blobClient = new BlobClient(_commonServices.Config.BlobStorageConnectionString, container, fileName);
        var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
        if (!blobExists.Value)
        {
            return await NotFoundData().ConfigureAwait(false);
        }

        var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream).ConfigureAwait(false);
        var fileData = memoryStream.ToArray();

        return new MediaResult(true, $"image/*", fileData);
    }

    [ComputeMethod]
    protected virtual Task<MediaResult> NotFoundData()
    {
        //var file = await File.ReadAllBytesAsync(@"C:\Users\John\Documents\AzerothMemories\AzerothMemories.WebBlazor\AzerothMemories.WebBlazor\wwwroot\header-banner.png").ConfigureAwait(false);
        return Task.FromResult<MediaResult>(null);
    }
}