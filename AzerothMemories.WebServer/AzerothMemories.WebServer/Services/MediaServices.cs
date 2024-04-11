using AzerothMemories.WebBlazor;
using Azure.Storage.Blobs;
using NodaTime.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Reflection;

namespace AzerothMemories.WebServer.Services;

public class MediaServices : IComputeService
{
    private readonly ILogger<MediaServices> _logger;
    private readonly CommonServices _commonServices;
    private readonly int[] _imageSizes;

    private const int SiteMapItemsPerFile = 20_000;
#if DEBUG
    private const string BaseUrl = "https://localhost:7048";
#else
    private const string BaseUrl = "https://memoriesofazeroth.com";
#endif

    public MediaServices(ILogger<MediaServices> logger, CommonServices commonServices)
    {
        _logger = logger;
        _commonServices = commonServices;
        _imageSizes = [600, 960, 1280, 1920, 2560, 0];
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetStaticMedia(Session session, string fileName)
    {
        using var _ = new MethodTimeLogger(_logger);
        return TryGetStaticMedia(fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetStaticMedia(string fileName)
    {
        using var _ = new MethodTimeLogger(_logger);
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
        using var _ = new MethodTimeLogger(_logger);
        var result = await TryGetBlobData(ZExtensions.BlobStaticMedia, "inv_misc_questionmark.jpg").ConfigureAwait(false);
        return result with { IsDefault = true };
    }

    [ComputeMethod]
    public virtual Task<MediaResult> TryGetUserAvatar(Session session, string fileName)
    {
        using var _ = new MethodTimeLogger(_logger);
        return TryGetUserAvatar(fileName);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserAvatar(string fileName)
    {
        using var _ = new MethodTimeLogger(_logger);
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
        using var _ = new MethodTimeLogger(_logger);
        var result = await TryGetBlobData(ZExtensions.BlobStaticMedia, "inv_misc_questionmark.jpg").ConfigureAwait(false);
        return result with { IsDefault = true };
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserUpload(Session session, string fileName, MediaSize size)
    {
        using var _ = new MethodTimeLogger(_logger);
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
        using var _ = new MethodTimeLogger(_logger);
        var blobData = await TryGetBlobData(ZExtensions.BlobStaticMedia, "inv_misc_questionmark.jpg").ConfigureAwait(false);
        return new MediaUserResult(blobData.LastModified, blobData.ETag, blobData.MediaType, blobData.MediaBytes, 0, 0) { IsDefault = true };
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetUserUpload(int accountId, string fileName, MediaSize size)
    {
        using var _ = new MethodTimeLogger(_logger);
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
        using var _ = new MethodTimeLogger(_logger);
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
        using var _ = new MethodTimeLogger(_logger);
        var blobData = await TryGetBlobData(ZExtensions.BlobUserUploads, fileName).ConfigureAwait(false);
        if (blobData == null)
        {
            return await TryGetUserUpload_Default().ConfigureAwait(false);
        }

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

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
        using var _ = new MethodTimeLogger(_logger);
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

    [ComputeMethod(AutoInvalidationDelay = 60 * 10)]
    public virtual async Task<int[]> GetSiteMapCounters()
    {
        using var _ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var accountMax = await database.Accounts.MaxAsync(x => (int?)x.Id).ConfigureAwait(false);
        var charactersMax = await database.Characters.MaxAsync(x => (int?)x.Id).ConfigureAwait(false);
        var guildsMax = await database.Guilds.MaxAsync(x => (int?)x.Id).ConfigureAwait(false);
        var postsMax = await database.Posts.MaxAsync(x => (int?)x.Id).ConfigureAwait(false);

        var results = new int[(int)SiteMapType.Count];

        results[(int)SiteMapType.Accounts] = accountMax == null ? 0 : accountMax.Value / SiteMapItemsPerFile + 1;
        results[(int)SiteMapType.Characters] = charactersMax == null ? 0 : charactersMax.Value / SiteMapItemsPerFile + 1;
        results[(int)SiteMapType.Guilds] = guildsMax == null ? 0 : guildsMax.Value / SiteMapItemsPerFile + 1;
        results[(int)SiteMapType.Posts] = postsMax == null ? 0 : postsMax.Value / SiteMapItemsPerFile + 1;

        return results;
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetSiteMapIndex()
    {
        using var _ = new MethodTimeLogger(_logger);
        var counters = await GetSiteMapCounters().ConfigureAwait(false);

        var maps = new List<(string Url, DateTime LastModified)>
        {
            new() { Url = "/sitemaps/sitemap-main.xml", LastModified = DateTime.UtcNow }
        };

        Add(SiteMapType.Accounts);
        Add(SiteMapType.Characters);
        Add(SiteMapType.Guilds);
        Add(SiteMapType.Posts);

        return await SiteMapHelper.BuildSiteMap(BaseUrl, "sitemapindex", "sitemap", maps).ConfigureAwait(false);

        void Add(SiteMapType siteMapType)
        {
            var count = counters[(int)siteMapType];
            var nameType = siteMapType.ToString();
            var results = Enumerable.Range(0, count).Select(x => ($"/sitemaps/sitemap-{char.ToLower(nameType[0]) + nameType[1..]}-{x}.xml", DateTime.UtcNow));

            maps.AddRange(results);
        }
    }

    [ComputeMethod(AutoInvalidationDelay = 60 * 10)]
    public virtual async Task<MediaResult> TryGetSiteMapMain()
    {
        using var _ = new MethodTimeLogger(_logger);
        var pages = new List<(string Url, DateTime LastModified)>
        {
            new() { Url = "/", LastModified = DateTime.Now }
        };

        var allComponents = typeof(App).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Microsoft.AspNetCore.Components.ComponentBase)));
        var allRoutedComponent = allComponents.Select(x => new { Type = x, Route = x.GetCustomAttributes<Microsoft.AspNetCore.Components.RouteAttribute>().FirstOrDefault() }).Where(x => x.Route != null).ToList();
        var toAddToSiteNap = allRoutedComponent.Where(x => x.Route.Template != "/" && x.Route.Template != "/admin" && !x.Route.Template.Contains('{') && !x.Route.Template.Contains('}')).ToList();

        foreach (var routedComponent in toAddToSiteNap)
        {
            pages.Add((routedComponent.Route.Template, DateTime.Now));
        }

        return await SiteMapHelper.BuildSiteMap(BaseUrl, "urlset", "url", pages).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult> TryGetSiteMapNamed(SiteMapType nameType, int fileIndex)
    {
        using var _ = new MethodTimeLogger(_logger);
        var counters = await _commonServices.MediaServices.GetSiteMapCounters().ConfigureAwait(false);
        if (fileIndex >= counters[(int)nameType])
        {
            return null;
        }

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var pages = new List<(string Url, DateTime LastModified)>();
        if (nameType == SiteMapType.Accounts)
        {
            var query = from r in database.Accounts.AsNoTracking()
                        orderby r.Id
                        select new { r.Id, r.Username };

            var items = await query.Skip(SiteMapItemsPerFile * fileIndex).Take(SiteMapItemsPerFile).ToArrayAsync().ConfigureAwait(false);

            foreach (var item in items)
            {
                pages.Add(($"/account/{item.Id}", DateTime.Now));
                pages.Add(($"/account/{item.Username}", DateTime.Now));
            }
        }
        else if (nameType == SiteMapType.Characters)
        {
            var query = from r in database.Characters.AsNoTracking()
                        where r.CharacterStatus == CharacterStatus2.None && r.UpdateRecord != null
                        orderby r.Id
                        select new { r.Id, r.MoaRef };

            var items = await query.Skip(SiteMapItemsPerFile * fileIndex).Take(SiteMapItemsPerFile).ToArrayAsync().ConfigureAwait(false);

            foreach (var item in items)
            {
                var moaRef = new MoaRef(item.MoaRef);

                pages.Add(($"/character/{item.Id}", DateTime.Now));
                pages.Add(($"/character/{moaRef.Region.ToInfo().TwoLettersLower}/{moaRef.Realm}/{moaRef.Name}", DateTime.Now));
            }
        }
        else if (nameType == SiteMapType.Guilds)
        {
            var query = from r in database.Guilds.AsNoTracking()
                        orderby r.Id
                        select new { r.Id, r.MoaRef };

            var items = await query.Skip(SiteMapItemsPerFile * fileIndex).Take(SiteMapItemsPerFile).ToArrayAsync().ConfigureAwait(false);

            foreach (var item in items)
            {
                var moaRef = new MoaRef(item.MoaRef);

                pages.Add(($"/guild/{item.Id}", DateTime.Now));
                pages.Add(($"/guild/{moaRef.Region.ToInfo().TwoLettersLower}/{moaRef.Realm}/{moaRef.Name}", DateTime.Now));
            }
        }
        else if (nameType == SiteMapType.Posts)
        {
            var query = from r in database.Posts.AsNoTracking()
                        where r.PostVisibility == 0 && r.DeletedTimeStamp == 0
                        orderby r.Id
                        select new { r.Id, r.AccountId };

            var items = await query.ToListAsync().ConfigureAwait(false);
            foreach (var item in items)
            {
                pages.Add(($"/post/{item.AccountId}/{item.Id}", DateTime.Now));
            }
        }
        else
        {
            return await TryGetSiteMapMain().ConfigureAwait(false);
        }

        return await SiteMapHelper.BuildSiteMap(BaseUrl, "urlset", "url", pages).ConfigureAwait(false);
    }
}