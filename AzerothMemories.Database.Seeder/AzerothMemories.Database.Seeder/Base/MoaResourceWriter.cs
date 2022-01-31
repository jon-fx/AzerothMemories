namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaResourceWriter
{
    private readonly IDbContextFactory<AppDbContext> _databaseProvider;
    private readonly Dictionary<string, BlizzardDataRecord> _serverSideResources;

    private readonly Dictionary<string, BlizzardDataRecord> _changedServerSideResources;
    private readonly Dictionary<string, Dictionary<string, string>> _allResourceStringsByLocal;

    public MoaResourceWriter(IDbContextFactory<AppDbContext> databaseProvider)
    {
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

        var results = await database.BlizzardData.ToArrayAsync();
        foreach (var result in results)
        {
            _serverSideResources.Add(result.Key, result);
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

    public void AddServerSideLocalizationMedia(PostTagType tagType, long tagId, string media)
    {
        var resource = GetOrCreateServerSideResource(tagType, tagId);
        if (resource.Media == media)
        {
            return;
        }

        resource.Media = media;

        OnServerSideRecordUpdated(true, resource);
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
        var newResources = _changedServerSideResources.Values.Where(x => x.Id == 0);
        var updatedResources = _changedServerSideResources.Values.Where(x => x.Id > 0);

        await using var database = await _databaseProvider.CreateDbContextAsync();

        foreach (var newResource in newResources)
        {
            database.BlizzardData.Add(newResource);
        }

        foreach (var updatedResource in updatedResources)
        {
            await database.BlizzardData.Where(x => x.Id == updatedResource.Id).UpdateAsync(x => new BlizzardDataRecord()
            {
                Name = new BlizzardDataRecordLocal
                {
                    EnUs = updatedResource.Name.EnUs,
                    KoKr = updatedResource.Name.KoKr,
                    FrFr = updatedResource.Name.FrFr,
                    DeDe = updatedResource.Name.DeDe,
                    ZhCn = updatedResource.Name.ZhCn,
                    EsEs = updatedResource.Name.EsEs,
                    ZhTw = updatedResource.Name.ZhTw,
                    EnGb = updatedResource.Name.EnGb,
                    EsMx = updatedResource.Name.EsMx,
                    RuRu = updatedResource.Name.RuRu,
                    PtBr = updatedResource.Name.PtBr,
                    ItIt = updatedResource.Name.ItIt,
                    PtPt = updatedResource.Name.PtPt,
                }
            });
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

        foreach (var record in realmData)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);

            noneDict.Add($"RealmSlug-{record.TagId}", record.Media);
        }

        foreach (var record in typeTags)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);

            noneDict.Add(record.Key, record.Key);
        }

        foreach (var record in mainTags)
        {
            SetExtensions.Update(record.Key, record.Name, clientSideDataDict);

            noneDict.Add(record.Key, record.Key);
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