using Azure.Storage.Blobs;
using NodaTime.Extensions;

namespace AzerothMemories.WebServer.Services;

public class MediaServices : IComputeService
{
    private readonly ILogger<MediaServices> _logger;
    private readonly CommonServices _commonServices;
    private readonly int[] _imageSizes;

    public MediaServices(ILogger<MediaServices> logger, CommonServices commonServices)
    {
        _logger = logger;
        _commonServices = commonServices;
        _imageSizes = new[] { 600, 960, 1280, 1920, 2560, 0 };
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetStaticMedia(Session session, string fileName)
    {
        return TryGetStaticMedia(fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetStaticMedia(string fileName)
    {
        var result = await TryGetBlobData(ZExtensions.BlobStaticMedia, fileName).ConfigureAwait(false);
        if (result != null)
        {
            return result;
        }

        return await TryGetMedia_Default().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<MediaResult> TryGetMedia_Default()
    {
        var result = await TryGetBlobData(ZExtensions.BlobStaticMedia, "inv_misc_questionmark.jpg").ConfigureAwait(false);
        return result with { IsDefault = true };
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetUserAvatar(Session session, string fileName)
    {
        return TryGetUserAvatar(fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserAvatar(string fileName)
    {
        var result = await TryGetBlobData(ZExtensions.BlobUserAvatars, fileName).ConfigureAwait(false);
        if (result != null)
        {
            return result;
        }

        return await TryGetAvatar_Default().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<MediaResult> TryGetAvatar_Default()
    {
        var result = await TryGetBlobData(ZExtensions.BlobStaticMedia, "inv_misc_questionmark.jpg").ConfigureAwait(false);
        return result with { IsDefault = true };
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserUpload(Session session, string fileName, MediaSize size)
    {
        var accountId = 0;
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (account != null)
        {
            accountId = account.Id;
        }

        var result = await TryGetUserUpload(accountId, fileName, size).ConfigureAwait(false);
        if (result.IsDefault)
        {
            return result;
        }

        //TODO:

        return result;
    }

    [ComputeMethod]
    protected virtual async Task<MediaUserResult> TryGetUserUpload_Default()
    {
        var blobData = await TryGetBlobData(ZExtensions.BlobStaticMedia, "inv_misc_questionmark.jpg").ConfigureAwait(false);
        return new MediaUserResult(blobData.LastModified, blobData.ETag, blobData.MediaType, blobData.MediaBytes, 0, 0) { IsDefault = true };
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserUpload(int accountId, string fileName, MediaSize size)
    {
        var blobData = await TryGetUserUpload(fileName, size).ConfigureAwait(false);
        if (blobData.IsDefault)
        {
            return blobData;
        }

        var postRecord = await _commonServices.PostServices.TryGetPostRecord(blobData.PostId).ConfigureAwait(false);
        if (postRecord.PostVisibility > 0)
        {
            if (accountId == 0)
            {
                return await TryGetUserUpload_Default().ConfigureAwait(false);
            }

            var canSeePost = await _commonServices.PostServices.CanAccountSeePost(accountId, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
            if (!canSeePost)
            {
                return await TryGetUserUpload_Default().ConfigureAwait(false);
            }
        }

        return blobData;
    }

    [ComputeMethod]
    protected virtual async Task<MediaUserResult> TryGetUserUpload(string fileName, MediaSize size)
    {
        var blobData = await TryGetUserUploadBlobData(fileName).ConfigureAwait(false);
        if (blobData.IsDefault)
        {
            return blobData;
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

                return new MediaUserResult(blobData.LastModified, blobData.ETag, blobData.MediaType, memoryStream.ToArray(), blobData.PostId, blobData.PostAccountId);
            }
        }

        return blobData;
    }

    [ComputeMethod]
    protected virtual async Task<MediaUserResult> TryGetUserUploadBlobData(string fileName)
    {
        var blobData = await TryGetBlobData(ZExtensions.BlobUserUploads, fileName).ConfigureAwait(false);
        if (blobData == null)
        {
            return await TryGetUserUpload_Default().ConfigureAwait(false);
        }

        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var postRecord = await database.UploadLogs.Where(x => x.BlobName == fileName).FirstOrDefaultAsync().ConfigureAwait(false);
        if (postRecord == null)
        {
            return await TryGetUserUpload_Default().ConfigureAwait(false);
        }

        if (postRecord.UploadStatus == AccountUploadLogStatus.Deleted || postRecord.UploadStatus == AccountUploadLogStatus.DeletePending)
        {
            return await TryGetUserUpload_Default().ConfigureAwait(false);
        }

        return new MediaUserResult(blobData.LastModified, blobData.ETag, blobData.MediaType, blobData.MediaBytes, postRecord.PostId.GetValueOrDefault(), postRecord.AccountId);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetBlobData(string container, string fileName)
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