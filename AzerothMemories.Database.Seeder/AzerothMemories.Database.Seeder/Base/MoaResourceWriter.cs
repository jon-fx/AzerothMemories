namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaResourceWriter
{
    private readonly DatabaseProvider _databaseProvider;
    private readonly Dictionary<string, BlizzardDataRecord> _serverSideResources;
    private readonly Dictionary<string, BlizzardDataRecord> _changedServerSideResources;
    private readonly Dictionary<string, Dictionary<string, string>> _allResourceStringsByLocal;

    public MoaResourceWriter(DatabaseProvider databaseProvider)
    {
        _databaseProvider = databaseProvider;
        _serverSideResources = new Dictionary<string, BlizzardDataRecord>();
        _changedServerSideResources = new Dictionary<string, BlizzardDataRecord>();
        _allResourceStringsByLocal = new Dictionary<string, Dictionary<string, string>>
        {
            {"None", new Dictionary<string, string>()}
        };
    }

    public void AddServerSideLocalizationName(PostTagType tagType, long tagId, BlizzardDataRecordLocal data)
    {
        var resource = GetOrCreateServerSideResource(tagType, tagId);
        var updated = resource.Name.Update(data);

        OnServerSideRecordUpdated(updated, resource);

        SetExtensions.Update(PostTagInfo.GetTagString(tagType, tagId), data, _allResourceStringsByLocal);
    }

    public void AddServerSideLocalizationMedia(PostTagType tagType, long tagId, string media)
    {
        var resource = GetOrCreateServerSideResource(tagType, tagId);
        var updated = resource.UpdateMedia(media);

        OnServerSideRecordUpdated(updated, resource);
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
            using (var database = _databaseProvider.GetDatabase())
            {
                value = database.BlizzardData.FirstOrDefault(x => x.Key == key);
            }

            if (value == null)
            {
                value = new BlizzardDataRecord(tagType, tagId);
            }

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

        await using var database = _databaseProvider.GetDatabase();
        await database.BulkCopyAsync(newResources);

        foreach (var resource in updatedResources)
        {
            await database.UpdateAsync(resource);
        }

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