using ActualLab.Collections;
using AzerothMemories.Database.Seeder.Base;
using AzerothMemories.WebBlazor.Blizzard;
using AzerothMemories.WebBlazor.Common;
using AzerothMemories.WebServer.Database;
using AzerothMemories.WebServer.Database.Records;
using KGySoft.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AzerothMemories.Database.Seeder.Import;

internal sealed class MoaDatabaseWriter
{
    private readonly ILogger<MoaDatabaseWriter> _logger;
    private readonly IDbContextFactory<AppDbContext> _databaseProvider;
    private readonly Dictionary<string, BlizzardData> _serverSideResources;

    public MoaDatabaseWriter(ILogger<MoaDatabaseWriter> logger, IDbContextFactory<AppDbContext> databaseProvider)
    {
        _logger = logger;
        _databaseProvider = databaseProvider;
        _serverSideResources = new Dictionary<string, BlizzardData>();
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
    }

    public async Task Save()
    {
        _logger.LogInformation("Begin Save");

        var groupedByTagType = _serverSideResources.Values.GroupBy(x => x.TagType).ToDictionary(x => x.Key, x => x.OrderBy(y => y.TagId).ToArray());

        var clientSideDataDict = new Dictionary<string, string>[(int)ServerSideLocale.Count];
        for (var i = 0; i < clientSideDataDict.Length; i++)
        {
            clientSideDataDict[i] = new Dictionary<string, string>();
        }

        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.Type].Select(x => new KeyValuePair<string, string>(x.Key, x.GetNameOrDefault(ServerSideLocale.En_Us))));
        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.Region].Select(x => new KeyValuePair<string, string>(x.Key, x.GetNameOrDefault(ServerSideLocale.En_Us))));
        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.Main].Select(x => new KeyValuePair<string, string>(x.Key, x.GetNameOrDefault(ServerSideLocale.En_Us))));
        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.Realm].Select(x => new KeyValuePair<string, string>(x.Key, x.GetNameOrDefault(ServerSideLocale.En_Us))));
        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.Realm].Select(x => new KeyValuePair<string, string>($"RealmSlug-{x.TagId}", x.Media!)).Where(x => !string.IsNullOrEmpty(x.Value)));
        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.CharacterRace].Select(x => new KeyValuePair<string, string>(x.Key, x.GetNameOrDefault(ServerSideLocale.En_Us))));
        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.CharacterClass].Select(x => new KeyValuePair<string, string>(x.Key, x.GetNameOrDefault(ServerSideLocale.En_Us))));
        clientSideDataDict[0].AddRange(groupedByTagType[PostTagType.CharacterClassSpecialization].Select(x => new KeyValuePair<string, string>(x.Key, x.GetNameOrDefault(ServerSideLocale.En_Us))));

        AddResourcesToClientDictionaries(groupedByTagType[PostTagType.Realm], clientSideDataDict);
        AddResourcesToClientDictionaries(groupedByTagType[PostTagType.Region], clientSideDataDict);

        AddResourcesToClientDictionaries(groupedByTagType[PostTagType.Type], clientSideDataDict);
        AddResourcesToClientDictionaries(groupedByTagType[PostTagType.Main], clientSideDataDict);

        AddResourcesToClientDictionaries(groupedByTagType[PostTagType.CharacterRace], clientSideDataDict);
        AddResourcesToClientDictionaries(groupedByTagType[PostTagType.CharacterClass], clientSideDataDict);
        AddResourcesToClientDictionaries(groupedByTagType[PostTagType.CharacterClassSpecialization], clientSideDataDict);

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

        var blizzardDataRecords = await database.BlizzardData.ToDictionaryAsync(x => x.Key, x => x);
        foreach (var serverSideResource in _serverSideResources.Values)
        {
            if (!blizzardDataRecords.TryGetValue(serverSideResource.Key, out var currentData))
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

        var blizzardRealmRecords = await database.BlizzardRealms.ToDictionaryAsync(x => x.RealmId, x => x);
        foreach (var realmData in groupedByTagType[PostTagType.Realm])
        {
            if (!blizzardRealmRecords.TryGetValue(realmData.TagId, out var currentData))
            {
                currentData = new BlizzardRealmRecord
                {
                    RealmId = realmData.TagId
                };

                database.Attach(currentData);
            }

            currentData.RealmSlug = realmData.Media ?? string.Empty;
            currentData.RealmNameEnGb = realmData.GetNameOrDefault(ServerSideLocale.En_Gb);
            currentData.RealmNameEnUs = realmData.GetNameOrDefault(ServerSideLocale.En_Us);
            currentData.RealmRegion = GetRealmRegionFromName(currentData.RealmNameEnGb);
            currentData.RealmVersion = GetRealmVersionFromName(currentData.RealmNameEnGb);
        }

        await database.SaveChangesAsync();

        var databaseItemCount = await database.BlizzardData.CountAsync();
        if (_serverSideResources.Count > databaseItemCount)
        {
            throw new NotImplementedException();
        }

        _logger.LogInformation("End Save");
    }

    private static BlizzardRegion GetRealmRegionFromName(string realmName)
    {
        foreach (var regionInfo in BlizzardRegionInfo.AllByTwoLetters)
        {
            var temp = regionInfo.Key;
            if (realmName.StartsWith(temp, StringComparison.OrdinalIgnoreCase))
            {
                return regionInfo.Value.Region;
            }
        }

        return BlizzardRegion.None;
    }

    private static BlizzardRealmVersion GetRealmVersionFromName(string realmName)
    {
        foreach (var realmVersion in BlizzardRealmVersionExt.AllRealmVersions)
        {
            var temp = realmVersion.GetRealmTagSuffix();
            if (string.IsNullOrWhiteSpace(temp))
            {
                continue;
            }
            if (realmName.EndsWith(temp))
            {
                return realmVersion;
            }
        }

        return BlizzardRealmVersion.Main;
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