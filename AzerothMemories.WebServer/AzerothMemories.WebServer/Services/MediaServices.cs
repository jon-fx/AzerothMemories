using Azure.Storage;
using Azure.Storage.Sas;
using System.Reflection;

namespace AzerothMemories.WebServer.Services;

public class MediaServices : IComputeService
{
    private readonly ILogger<MediaServices> _logger;
    private readonly CommonServices _commonServices;

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
    }

    [ComputeMethod(AutoInvalidationDelay = 60 * 10)]
    public virtual async Task<int[]> GetSiteMapCounters()
    {
        using var _ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        int accountMax;
        int charactersMax;
        int guildsMax;
        int postsMax;

        {
            var query = from r in database.Accounts.IgnoreAutoIncludes().AsNoTracking()
                        orderby r.Id
                        select new { r.Id, r.Username };

            accountMax = await query.CountAsync().ConfigureAwait(false);
        }
        {
            var query = from r in database.Characters.IgnoreAutoIncludes().AsNoTracking()
                        where r.CharacterStatus == CharacterStatus2.None && r.UpdateRecord != null
                        orderby r.Id
                        select new { r.Id, r.MoaRef };

            charactersMax = await query.CountAsync().ConfigureAwait(false);
        }
        {
            var query = from r in database.Guilds.IgnoreAutoIncludes().AsNoTracking()
                        orderby r.Id
                        select new { r.Id, r.MoaRef };

            guildsMax = await query.CountAsync().ConfigureAwait(false);
        }
        {
            var query = from r in database.Posts.IgnoreAutoIncludes().AsNoTracking()
                        where r.PostVisibility == 0 && r.DeletedTimeStamp == 0
                        orderby r.Id
                        select new { r.Id, r.AccountId };

            postsMax = await query.CountAsync().ConfigureAwait(false);
        }

        var results = new int[(int)SiteMapType.Count];

        results[(int)SiteMapType.Accounts] = accountMax / SiteMapItemsPerFile + 1;
        results[(int)SiteMapType.Characters] = charactersMax / SiteMapItemsPerFile + 1;
        results[(int)SiteMapType.Guilds] = guildsMax / SiteMapItemsPerFile + 1;
        results[(int)SiteMapType.Posts] = postsMax / SiteMapItemsPerFile + 1;

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

        Add(SiteMapType.Posts);
        Add(SiteMapType.Accounts);
        Add(SiteMapType.Characters);
        Add(SiteMapType.Guilds);

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

        var allComponents = typeof(Program).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Microsoft.AspNetCore.Components.ComponentBase)));
        var allRoutedComponent = allComponents.Select(x => new { Type = x, Route = x.GetCustomAttributes<Microsoft.AspNetCore.Components.RouteAttribute>().FirstOrDefault() }).Where(x => x.Route != null).ToList();
        var toAddToSiteNap = allRoutedComponent.Where(x => x.Route != null && x.Route.Template != "/" && x.Route.Template != "/admin" && !x.Route.Template.Contains('{') && !x.Route.Template.Contains('}')).ToList();

        foreach (var routedComponent in toAddToSiteNap)
        {
            if (routedComponent.Route == null)
            {
                continue;
            }

            pages.Add((routedComponent.Route.Template, DateTime.Now));
        }

        return await SiteMapHelper.BuildSiteMap(BaseUrl, "urlset", "url", pages).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<MediaResult?> TryGetSiteMapNamed(SiteMapType nameType, int fileIndex)
    {
        using var _ = new MethodTimeLogger(_logger, $"TryGetSiteMapNamed - nameType:{nameType} - fileIndex:{fileIndex}");
        var counters = await GetSiteMapCounters().ConfigureAwait(false);
        if (fileIndex >= counters[(int)nameType])
        {
            return null;
        }

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var pages = new List<(string Url, DateTime LastModified)>();
        if (nameType == SiteMapType.Accounts)
        {
            var query = from r in database.Accounts.IgnoreAutoIncludes().AsNoTracking()
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
            var query = from r in database.Characters.IgnoreAutoIncludes().AsNoTracking()
                        where r.CharacterStatus == CharacterStatus2.None && r.UpdateRecord != null
                        orderby r.Id
                        select new { r.Id, r.MoaRef };

            var items = await query.Skip(SiteMapItemsPerFile * fileIndex).Take(SiteMapItemsPerFile).ToArrayAsync().ConfigureAwait(false);

            foreach (var item in items)
            {
                var moaRef = new MoaRef(item.MoaRef);

                pages.Add(($"/character/{item.Id}", DateTime.Now));
                pages.Add(($"/character/{moaRef.Region.ToInfo().TwoLettersLower}/{moaRef.RealmVersion.ToValue()}/{moaRef.Realm}/{moaRef.Name}", DateTime.Now));
            }
        }
        else if (nameType == SiteMapType.Guilds)
        {
            var query = from r in database.Guilds.IgnoreAutoIncludes().AsNoTracking()
                        orderby r.Id
                        select new { r.Id, r.MoaRef };

            var items = await query.Skip(SiteMapItemsPerFile * fileIndex).Take(SiteMapItemsPerFile).ToArrayAsync().ConfigureAwait(false);

            foreach (var item in items)
            {
                var moaRef = new MoaRef(item.MoaRef);

                pages.Add(($"/guild/{item.Id}", DateTime.Now));
                pages.Add(($"/guild/{moaRef.Region.ToInfo().TwoLettersLower}/{moaRef.RealmVersion.ToValue()}/{moaRef.Realm}/{moaRef.Name}", DateTime.Now));
            }
        }
        else if (nameType == SiteMapType.Posts)
        {
            var query = from r in database.Posts.IgnoreAutoIncludes().AsNoTracking()
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

    [ComputeMethod(AutoInvalidationDelay = 60 * 60 * 20)]
    public virtual Task<string> TryGetBlobWithToken(string blobName)
    {
        var containerName = "images";
        var isAvatar = blobName.StartsWith(ZExtensions.CustomUserAvatarPathPrefix);
        if (isAvatar)
        {
            containerName = "avatars";
            blobName = blobName.Replace(ZExtensions.BlobUserAvatarsStoragePath, "");
        }

        var blobSasBuilder = new BlobSasBuilder
        {
            BlobName = blobName,
            BlobContainerName = containerName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
        };

        blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

        var storageSharedKeyCredential = new StorageSharedKeyCredential(_commonServices.Config.BlobStorageAccount, _commonServices.Config.BlobStorageAccountKey);
        var sasQueryParameters = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential);

        if (isAvatar)
        {
            blobName = $"{ZExtensions.BlobUserAvatarsStoragePath}{blobName}";
        }

        return Task.FromResult($"{blobName}?{sasQueryParameters}");
    }
}