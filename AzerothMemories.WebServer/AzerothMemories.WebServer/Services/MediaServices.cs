using Azure.Storage.Blobs;
using NodaTime.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
public class MediaServices : DbServiceBase<AppDbContext>
{
    private readonly CommonServices _commonServices;
    private readonly int[] _imageSizes;

    public MediaServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
        _imageSizes = new[] { 600, 960, 1280, 1920, 2560, 0 };
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetMedia(Session session, string fileName)
    {
        return TryGetMedia(fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetMedia(string fileName)
    {
        var result = await TryGetBlobData(ZExtensions.BlobMedia, fileName).ConfigureAwait(false);
        if (result != null)
        {
            return result;
        }

        return await TryGetMedia_Default().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual Task<MediaResult> TryGetMedia_Default()
    {
        return TryGetBlobData(ZExtensions.BlobMedia, "inv_misc_questionmark.jpg");
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetAvatar(Session session, string fileName)
    {
        return TryGetAvatar(fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetAvatar(string fileName)
    {
        var result = await TryGetBlobData(ZExtensions.BlobAvatars, fileName).ConfigureAwait(false);
        if (result != null)
        {
            return result;
        }

        return await TryGetAvatar_Default().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual Task<MediaResult> TryGetAvatar_Default()
    {
        return Task.FromResult((MediaResult)null);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserMedia(Session session, string fileName, MediaSize size)
    {
        var accountId = 0;
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (account != null)
        {
            accountId = account.Id;
        }

        return await TryGetUserMedia(accountId, fileName, size).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserMedia(int accountId, string fileName, MediaSize size)
    {
        var blobData = await TryGetBlobData(ZExtensions.BlobImages, fileName, size).ConfigureAwait(false);
        if (blobData == null)
        {
            return null;
        }

        return new MediaResult(blobData.LastModified, blobData.ETag, blobData.MediaType, blobData.MediaBytes);
    }

    [ComputeMethod]
    protected virtual async Task<MediaResult> TryGetBlobData(string container, string fileName, MediaSize size)
    {
        var blobData = await TryGetBlobData(ZExtensions.BlobImages, fileName).ConfigureAwait(false);
        if (blobData == null)
        {
            return null;
        }

        var width = _imageSizes[(int)size];
        if (width > 0 && fileName.EndsWith(".jpg"))
        {
            using var image = Image.Load(blobData.MediaBytes);
            if (image.Width > width)
            {
                image.Mutate(x => x.Resize(width, 0));

                await using var memoryStream = new MemoryStream();
                await image.SaveAsJpegAsync(memoryStream).ConfigureAwait(false);

                return new MediaResult(blobData.LastModified, blobData.ETag, blobData.MediaType, memoryStream.ToArray());
            }
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