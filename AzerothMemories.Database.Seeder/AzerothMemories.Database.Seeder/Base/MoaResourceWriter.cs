using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaResourceWriter
{
    private readonly WowTools _wowTools;
    private readonly ILogger<MoaResourceWriter> _logger;
    private readonly Dictionary<string, BlizzardData> _serverSideResources;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public MoaResourceWriter(WowTools wowTools, ILogger<MoaResourceWriter> logger)
    {
        _wowTools = wowTools;
        _logger = logger;
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
        _logger.LogInformation("Loading Resources from json...");

        var jsonRecords = await SeederConfig.LoadAllJsonRecords();
        foreach (var jsonRecord in jsonRecords)
        {
            _serverSideResources.Add(jsonRecord.Key, jsonRecord.Value);
        }

        _logger.LogInformation($"Loaded Resources: {_serverSideResources.Count}");

        var iconName = "inv_misc_questionmark";
        var fileInfo = SeederConfig.GetLocalMediaFileInfo(iconName);
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
        if (string.IsNullOrEmpty(mediaPath))
        {
            return;
        }

        var fileInfo = SeederConfig.GetLocalMediaFileInfo(Path.GetFileNameWithoutExtension(mediaPath));
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

        var fileInfo = SeederConfig.GetLocalMediaFileInfo(iconName);
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

        var groupedByTagType = _serverSideResources.Values.GroupBy(x => x.TagType).ToDictionary(x => x.Key, x => x.OrderBy(y => y.TagId).ToArray());
        foreach (var tagTypeGroup in groupedByTagType)
        {
            var tagType = tagTypeGroup.Key;
            var groupByTagId = tagTypeGroup.Value.GroupBy(x => x.TagId / 1000).ToDictionary(x => x.Key, x => x.OrderBy(y => y.TagId).ToArray());

            _logger.LogInformation($"Saving {tagTypeGroup.Value.Length} {tagType} - ({groupByTagId.Count} files)");

            foreach (var tagIdGroup in groupByTagId)
            {
                var outputFile = Path.Combine(SeederConfig.JsonDataPath, $"{tagType}-{tagIdGroup.Key}.json");

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                var items = tagIdGroup.Value.ToArray();

                await using var fileStream = File.Create(outputFile);
                await JsonSerializer.SerializeAsync(fileStream, items, _jsonSerializerOptions);
            }
        }

        _logger.LogInformation("End Save");
    }
}