using Stl.Collections;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaResourceWriter
{
    private readonly WowTools _wowTools;
    private readonly ILogger<MoaResourceWriter> _logger;
    private readonly IDbContextFactory<AppDbContext> _databaseProvider;
    private readonly Dictionary<string, BlizzardDataRecord> _serverSideResources;

    private readonly Dictionary<string, BlizzardDataRecord> _changedServerSideResources;
    private readonly Dictionary<string, Dictionary<string, string>> _allResourceStringsByLocal;

    public MoaResourceWriter(WowTools wowTools, ILogger<MoaResourceWriter> logger, IDbContextFactory<AppDbContext> databaseProvider)
    {
        _wowTools = wowTools;
        _logger = logger;
        _databaseProvider = databaseProvider;
        _serverSideResources = new Dictionary<string, BlizzardDataRecord>();
        _changedServerSideResources = new Dictionary<string, BlizzardDataRecord>();
        _allResourceStringsByLocal = new Dictionary<string, Dictionary<string, string>>
        {
            {"None", new Dictionary<string, string>()}
        };
    }

    public async Task Initialize()
    {
        await using var database = await _databaseProvider.CreateDbContextAsync();

        _logger.LogInformation("Loading Resources from database...");

        var results = await database.BlizzardData.AsNoTracking().ToArrayAsync();
        foreach (var result in results)
        {
            _serverSideResources.Add(result.Key, result);
        }

        _logger.LogInformation($"Loaded Resources: {_serverSideResources.Count}");

        var iconName = "inv_misc_questionmark";
        var fileInfo = GetLocalMediaFileInfo(iconName);
        if (!fileInfo.Exists)
        {
            await TryDownloadImage(fileInfo.FullName, new[] { "https://render.worldofwarcraft.com/eu/icons/56/inv_misc_questionmark.jpg" });
        }
    }

    public void AddServerSideLocalizationName(PostTagType tagType, long tagId, BlizzardDataRecordLocal data)
    {
        var resource = GetOrCreateServerSideResource(tagType, tagId);
        var changed = false;

        CheckAndChange(() => resource.Name.EnUs, x => resource.Name.EnUs = x, data.EnUs, ref changed);
        CheckAndChange(() => resource.Name.KoKr, x => resource.Name.KoKr = x, data.KoKr, ref changed);
        CheckAndChange(() => resource.Name.FrFr, x => resource.Name.FrFr = x, data.FrFr, ref changed);
        CheckAndChange(() => resource.Name.DeDe, x => resource.Name.DeDe = x, data.DeDe, ref changed);
        CheckAndChange(() => resource.Name.ZhCn, x => resource.Name.ZhCn = x, data.ZhCn, ref changed);
        CheckAndChange(() => resource.Name.EsEs, x => resource.Name.EsEs = x, data.EsEs, ref changed);
        CheckAndChange(() => resource.Name.ZhTw, x => resource.Name.ZhTw = x, data.ZhTw, ref changed);
        CheckAndChange(() => resource.Name.EnGb, x => resource.Name.EnGb = x, data.EnGb, ref changed);

        CheckAndChange(() => resource.Name.EsMx, x => resource.Name.EsMx = x, data.EsMx, ref changed);
        CheckAndChange(() => resource.Name.RuRu, x => resource.Name.RuRu = x, data.RuRu, ref changed);
        CheckAndChange(() => resource.Name.PtBr, x => resource.Name.PtBr = x, data.PtBr, ref changed);
        CheckAndChange(() => resource.Name.ItIt, x => resource.Name.ItIt = x, data.ItIt, ref changed);
        CheckAndChange(() => resource.Name.PtPt, x => resource.Name.PtPt = x, data.PtPt, ref changed);

        OnServerSideRecordUpdated(changed, resource);

        SetExtensions.Update(PostTagInfo.GetTagString(tagType, tagId), data, _allResourceStringsByLocal);
    }

    private void CheckAndChange(Func<string> getterFunc, Action<string> setterFunc, string newValue, ref bool changed)
    {
        var current = getterFunc();
        if (current == newValue)
        {
            return;
        }

        setterFunc(newValue);
        changed = true;
    }

    public void TryAddServerSideLocalizationMedia(PostTagType tagType, long tagId, string mediaPath)
    {
        Exceptions.ThrowIf(string.IsNullOrEmpty(mediaPath));

        var fileInfo = GetLocalMediaFileInfo(Path.GetFileNameWithoutExtension(mediaPath));
        if (tagType == PostTagType.Realm)
        {
        }
        else if (!fileInfo.Exists)
        {
            throw new NotImplementedException();
        }
        else
        {
            mediaPath = fileInfo.Name;
        }

        var resource = GetOrCreateServerSideResource(tagType, tagId);
        if (resource.Media == mediaPath)
        {
            return;
        }

        resource.Media = mediaPath;

        OnServerSideRecordUpdated(true, resource);
    }

    public FileInfo GetLocalMediaFileInfo(string mediaPath)
    {
        var localPath = $@"C:\Users\John\Desktop\Stuff\BlizzardData\Media\{mediaPath}.jpg";
        return new FileInfo(localPath);
    }

    public async Task TryAddServerSideLocalizationMedia(PostTagType tagType, long tagId, int mediaId)
    {
        if (mediaId == 0)
        {
            return;
        }

        string remotePath = null;
        if (_wowTools.TryGetIconName(mediaId, out var iconName))
        {
            remotePath = $"https://render.worldofwarcraft.com/eu/icons/56/{iconName}.jpg";
        }
        else
        {
            iconName = mediaId.ToString();
        }

        var fileInfo = GetLocalMediaFileInfo(iconName);
        if (fileInfo.Exists && fileInfo.Length > 0)
        {
        }
        else
        {
            var pathsToTry = new[]
            {
                remotePath,
                $"https://wow.zamimg.com/images/wow/icons/large/{iconName.Replace(" ", "-")}.jpg",
                $"https://wow.zamimg.com/images/wow/icons/large/{mediaId}.jpg",
            };

            var result = await TryDownloadImage(fileInfo.FullName, pathsToTry);
            if (result)
            {
            }
            else
            {
                _logger.LogWarning($"{tagType}: {tagId} - Missing Icon: {mediaId}");
                return;
            }
        }

        TryAddServerSideLocalizationMedia(tagType, tagId, Path.GetFileNameWithoutExtension(fileInfo.Name));
    }

    private async Task<bool> TryDownloadImage(string fileName, string[] pathsToTry)
    {
        await using var memoryStream = new MemoryStream();

        foreach (var path in pathsToTry)
        {
            if (await TryDownloadImage(memoryStream, path))
            {
                break;
            }
        }

        var buffer = memoryStream.ToArray();
        if (buffer.Length > 0)
        {
            await File.WriteAllBytesAsync(fileName, memoryStream.ToArray());

            return true;
        }

        return false;
    }

    private async Task<bool> TryDownloadImage(MemoryStream fileStream, string remotePath)
    {
        if (string.IsNullOrEmpty(remotePath))
        {
            return false;
        }

        using var client = new HttpClient();
        using var response = await client.GetAsync(remotePath);
        if (response.IsSuccessStatusCode)
        {
            await response.Content.CopyToAsync(fileStream);
        }

        return response.IsSuccessStatusCode;
    }

    public bool GetClientSideLocalizationData(string locale, string key, out string value)
    {
        if (!_allResourceStringsByLocal.TryGetValue(locale, out var dict))
        {
            value = null;
            return false;
        }

        return dict.TryGetValue(key, out value);
    }

    public BlizzardDataRecord GetOrCreateServerSideResource(PostTagType tagType, long tagId)
    {
        var key = PostTagInfo.GetTagString(tagType, tagId);
        if (!_serverSideResources.TryGetValue(key, out var value))
        {
            value = new BlizzardDataRecord(tagType, tagId);

            Exceptions.ThrowIf(value.TagId != tagId);
            Exceptions.ThrowIf(value.TagType != tagType);
            Exceptions.ThrowIf(value.Key != key);

            _serverSideResources.Add(key, value);
        }

        return value;
    }

    private void OnServerSideRecordUpdated(bool updated, BlizzardDataRecord resource)
    {
        if (updated)
        {
            _changedServerSideResources[resource.Key] = resource;
        }
    }

    public async Task Save()
    {
        _logger.LogInformation("Begin Save");

        var newResources = _changedServerSideResources.Values.Where(x => x.Id == 0).ToArray();
        var updatedResources = _changedServerSideResources.Values.Where(x => x.Id > 0).ToArray();

        _logger.LogInformation($"New Resources: {newResources.Length}");
        _logger.LogInformation($"Updated Resources: {newResources.Length}");

        await using var database = await _databaseProvider.CreateDbContextAsync();

        foreach (var newResource in newResources)
        {
            database.BlizzardData.Add(newResource);
        }
        
        foreach (var updatedResource in updatedResources)
        {
            database.BlizzardData.Update(updatedResource);
        }

        await database.SaveChangesAsync();

        var typeTags = await database.BlizzardData.Where(x => x.TagType == PostTagType.Type).OrderBy(x => x.TagId).ToListAsync();
        var mainTags = await database.BlizzardData.Where(x => x.TagType == PostTagType.Main).OrderBy(x => x.TagId).ToListAsync();

        var realmData = await database.BlizzardData.Where(x => x.TagType == PostTagType.Realm).OrderBy(x => x.TagId).ToListAsync();
        var characterClassData = await database.BlizzardData.Where(x => x.TagType == PostTagType.CharacterClass).OrderBy(x => x.TagId).ToListAsync();
        var characterRaceData = await database.BlizzardData.Where(x => x.TagType == PostTagType.CharacterRace).OrderBy(x => x.TagId).ToListAsync();
        var characterSpecData = await database.BlizzardData.Where(x => x.TagType == PostTagType.CharacterClassSpecialization).OrderBy(x => x.TagId).ToListAsync();

        var noneDict = new Dictionary<string, string>();
        var clientSideDataDict = new Dictionary<string, Dictionary<string, string>>
        {
            {"None", noneDict}
        };

        noneDict.AddRange(typeTags.Select(x => new KeyValuePair<string, string>(x.Key, x.Key)));
        noneDict.AddRange(mainTags.Select(x => new KeyValuePair<string, string>(x.Key, x.Key)));
        noneDict.AddRange(realmData.Select(x => new KeyValuePair<string, string>($"RealmSlug-{x.TagId}", x.Media)).Where(x => !string.IsNullOrEmpty(x.Value)));

        foreach (var record in realmData)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);
        }

        foreach (var record in typeTags)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);
        }

        foreach (var record in mainTags)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);
        }

        foreach (var record in characterRaceData)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);
        }

        foreach (var record in characterClassData)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);
        }

        foreach (var record in characterSpecData)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);
        }

        foreach (var dict in clientSideDataDict)
        {
            using var writer = CreateResourceWriter(dict.Key);
            foreach (var kvp in dict.Value)
            {
                writer.AddResource(kvp.Key, kvp.Value);
            }

            writer.Generate();
        }

        _logger.LogInformation("End Save");
    }

    private ResXResourceWriter CreateResourceWriter(string key)
    {
        var filePath = @"..\..\..\AzerothMemories.WebBlazor\AzerothMemories.WebBlazor\Resources\BlizzardResources.";

        if (key == "None")
        {
        }
        else if (key.Contains("_"))
        {
            var localeText = key.Replace('_', '-');
            var locale = new CultureInfo(localeText);

            filePath += $"{locale.Name}.";
        }

        filePath += "resx";

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return new ResXResourceWriter(filePath);
    }
}