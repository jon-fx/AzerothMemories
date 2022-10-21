using Stl.Collections;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaResourceWriter
{
    private static readonly string _jsonDataPath = @$"C:\Users\John\Desktop\Stuff\BlizzardData\JSON Data\";

    private readonly WowTools _wowTools;
    private readonly ILogger<MoaResourceWriter> _logger;
    private readonly IDbContextFactory<AppDbContext> _databaseProvider;
    private readonly Dictionary<string, BlizzardData> _serverSideResources;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public MoaResourceWriter(WowTools wowTools, ILogger<MoaResourceWriter> logger, IDbContextFactory<AppDbContext> databaseProvider)
    {
        _wowTools = wowTools;
        _logger = logger;
        _databaseProvider = databaseProvider;
        _serverSideResources = new Dictionary<string, BlizzardData>();
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task Initialize()
    {
        _logger.LogInformation("Loading Resources from database...");

        for (var i = 0; i < (int)PostTagType.Count; i++)
        {
            var tag = (PostTagType)i;
            var file = Path.Combine(_jsonDataPath, $"{tag}.json");

            if (!Enum.IsDefined(tag))
            {
                continue;
            }

            if (!File.Exists(file))
            {
                continue;
            }

            await using var stream = File.OpenRead(file);
            var blizzardDataSet = JsonSerializer.Deserialize<BlizzardData[]>(stream);

            Exceptions.ThrowIf(blizzardDataSet == null);

            foreach (var blizzardData in blizzardDataSet)
            {
                _serverSideResources.Add(blizzardData.Key, blizzardData);
            }
        }

        _logger.LogInformation($"Loaded Resources: {_serverSideResources.Count}");

        var iconName = "inv_misc_questionmark";
        var fileInfo = GetLocalMediaFileInfo(iconName);
        if (!fileInfo.Exists)
        {
            await TryDownloadImage(fileInfo.FullName, new[] { "https://render.worldofwarcraft.com/eu/icons/56/inv_misc_questionmark.jpg" });
        }
    }

    public void AddServerSideLocalizationName(PostTagType tagType, int tagId, string[] data)
    {
        var resource = GetOrCreateServerSideResource(tagType, tagId);

        for (var i = 0; i < data.Length; i++)
        {
            resource.Names[i] = data[i];
        }
    }

    public void TryAddServerSideLocalizationMedia(PostTagType tagType, int tagId, string mediaPath)
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
    }

    public FileInfo GetLocalMediaFileInfo(string mediaPath)
    {
        var localPath = $@"C:\Users\John\Desktop\Stuff\BlizzardData\Media\{mediaPath}.jpg";
        return new FileInfo(localPath);
    }

    public async Task TryAddServerSideLocalizationMedia(PostTagType tagType, int tagId, int mediaId)
    {
        if (mediaId == 0)
        {
            return;
        }

        string remotePath = null;
        if (_wowTools.Main.TryGetIconName(mediaId, out var iconName))
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

    public BlizzardData GetOrCreateServerSideResource(PostTagType tagType, int tagId)
    {
        var key = PostTagInfo.GetTagString(tagType, tagId);
        if (!_serverSideResources.TryGetValue(key, out var value))
        {
            value = new BlizzardData(tagType, tagId);

            Exceptions.ThrowIf(value.TagId != tagId);
            Exceptions.ThrowIf(value.TagType != tagType);
            Exceptions.ThrowIf(value.Key != key);

            _serverSideResources.Add(key, value);
        }

        return value;
    }

    public bool TryGetServerSideResource(PostTagType tagType, int tagId, out BlizzardData dataRecord)
    {
        var key = PostTagInfo.GetTagString(tagType, tagId);

        return _serverSideResources.TryGetValue(key, out dataRecord);
    }

    public async Task Save()
    {
        _logger.LogInformation("Begin Save");

        var groups = _serverSideResources.Values.GroupBy(x => x.TagType).ToDictionary(x => x.Key, x => x.OrderBy(y => y.TagId).ToArray());
        foreach (var group in groups)
        {
            var key = group.Key;
            var outputFile = Path.Combine(_jsonDataPath, $"{key}.json");

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            var items = group.Value.ToArray();

            await using var fileStream = File.Create(outputFile);
            await JsonSerializer.SerializeAsync(fileStream, items, _jsonSerializerOptions);
        }

        var clientSideDataDict = new Dictionary<string, string>[(int)ServerSideLocale.Count];
        for (var i = 0; i < clientSideDataDict.Length; i++)
        {
            clientSideDataDict[i] = new Dictionary<string, string>();
        }

        clientSideDataDict[0].AddRange(groups[PostTagType.Type].Select(x => new KeyValuePair<string, string>(x.Key, x.Key)));
        clientSideDataDict[0].AddRange(groups[PostTagType.Region].Select(x => new KeyValuePair<string, string>(x.Key, x.Key)));
        clientSideDataDict[0].AddRange(groups[PostTagType.Main].Select(x => new KeyValuePair<string, string>(x.Key, x.Key)));
        clientSideDataDict[0].AddRange(groups[PostTagType.Realm].Select(x => new KeyValuePair<string, string>($"RealmSlug-{x.TagId}", x.Media)).Where(x => !string.IsNullOrEmpty(x.Value)));

        AddResourcesToClientDictionaries(groups[PostTagType.Realm], clientSideDataDict);
        AddResourcesToClientDictionaries(groups[PostTagType.Region], clientSideDataDict);

        AddResourcesToClientDictionaries(groups[PostTagType.Type], clientSideDataDict);
        AddResourcesToClientDictionaries(groups[PostTagType.Main], clientSideDataDict);

        AddResourcesToClientDictionaries(groups[PostTagType.CharacterRace], clientSideDataDict);
        AddResourcesToClientDictionaries(groups[PostTagType.CharacterClass], clientSideDataDict);
        AddResourcesToClientDictionaries(groups[PostTagType.CharacterClassSpecialization], clientSideDataDict);

        for (var i = 0; i < clientSideDataDict.Length; i++)
        {
            using var writer = CreateResourceWriter((ServerSideLocale)i);
            foreach (var kvp in clientSideDataDict[i])
            {
                writer.AddResource(kvp.Key, kvp.Value);
            }

            writer.Generate();
        }

        await using var database = await _databaseProvider.CreateDbContextAsync();

        var results = await database.BlizzardData.ToDictionaryAsync(x => x.Key, x => x);
        foreach (var serverSideResource in _serverSideResources.Values)
        {
            if (!results.TryGetValue(serverSideResource.Key, out var currentData))
            {
                currentData = new BlizzardDataRecord(serverSideResource.TagType, serverSideResource.TagId);

                database.Attach(currentData);
            }

            currentData.Media = serverSideResource.Media;
            currentData.MinTagTime = serverSideResource.MinTagTime;

            currentData.Name.EnUs = serverSideResource.GetNameOrDefault(ServerSideLocale.En_Us);
            currentData.Name.EsMx = serverSideResource.GetNameOrDefault(ServerSideLocale.Es_Mx);
            currentData.Name.PtBr = serverSideResource.GetNameOrDefault(ServerSideLocale.Pt_Br);
            currentData.Name.EnGb = serverSideResource.GetNameOrDefault(ServerSideLocale.En_Gb);

            currentData.Name.EsEs = serverSideResource.GetNameOrDefault(ServerSideLocale.Es_Es);
            currentData.Name.FrFr = serverSideResource.GetNameOrDefault(ServerSideLocale.Fr_Fr);
            currentData.Name.RuRu = serverSideResource.GetNameOrDefault(ServerSideLocale.Ru_Ru);
            currentData.Name.DeDe = serverSideResource.GetNameOrDefault(ServerSideLocale.De_De);

            currentData.Name.PtPt = serverSideResource.GetNameOrDefault(ServerSideLocale.Pt_Pt);
            currentData.Name.ItIt = serverSideResource.GetNameOrDefault(ServerSideLocale.It_It);

            currentData.Name.KoKr = serverSideResource.GetNameOrDefault(ServerSideLocale.Ko_Kr);
            currentData.Name.ZhTw = serverSideResource.GetNameOrDefault(ServerSideLocale.Zh_Tw);
            currentData.Name.ZhCn = serverSideResource.GetNameOrDefault(ServerSideLocale.Zh_Cn);
        }

        if (database.BlizzardData.Count() != _serverSideResources.Count)
        {
            throw new NotImplementedException();
        }

        await database.SaveChangesAsync();

        _logger.LogInformation("End Save");
    }

    private void AddResourcesToClientDictionaries(BlizzardData[] allBlizzardData, Dictionary<string, string>[] clientSideDataDict)
    {
        foreach (var blizzardData in allBlizzardData)
        {
            var defaultValue = blizzardData.Names[(int)ServerSideLocale.En_Us];
            if (string.IsNullOrWhiteSpace(defaultValue))
            {
                throw new NotImplementedException();
            }

            for (var i = (int)ServerSideLocale.En_Us; i < (int)ServerSideLocale.Count; i++)
            {
                var newValue = blizzardData.Names[i];
                clientSideDataDict[i].Add(blizzardData.Key, string.IsNullOrWhiteSpace(newValue) ? defaultValue : newValue);
            }
        }
    }

    private ResXResourceWriter CreateResourceWriter(ServerSideLocale key)
    {
        var filePath = @"..\..\..\AzerothMemories.WebBlazor\AzerothMemories.WebBlazor\Resources\BlizzardResources.";

        if (key == ServerSideLocale.None)
        {
        }
        else
        {
            var keyStr = key.ToString();
            if (keyStr.Length != 5)
            {
                throw new NotImplementedException();
            }

            var localeText = $"{keyStr[0]}{keyStr[1]}-{keyStr[3]}{keyStr[4]}";
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