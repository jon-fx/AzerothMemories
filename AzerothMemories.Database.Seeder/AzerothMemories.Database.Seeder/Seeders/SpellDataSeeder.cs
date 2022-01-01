namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class SpellDataSeeder : GenericBase<SpellDataSeeder>
{
    public SpellDataSeeder(ILogger<SpellDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("SpellName", "ID", ref data);
        WowTools.LoadDataFromWowTools("SpellMisc", "SpellID", ref data, "engb", new[] { "ID", "SpellID", "SpellIconFileDataID" });

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddLocalizationData($"SpellName-{reference.Id}", reference.GetLocalised("Name_lang"));

            if (reference.TryGetData<int>("SpellIconFileDataID", out var iconId))
            {
                if (iconId == 0)
                {
                }
                else if (WowTools.TryGetIconName(iconId, out var iconName))
                {
                    var newValue = $"https://render.worldofwarcraft.com/eu/icons/56/{iconName}.jpg";
                    ResourceWriter.AddCommonLocalizationData($"SpellIconMediaPath-{reference.Id}", newValue);
                }
                else
                {
                    Logger.LogWarning($"Spell: {reference.Id} - Missing Icon :{iconId}");
                }
            }
            else
            {
                Logger.LogWarning($"Spell: {reference.Id} - Missing SpellIconFileDataID");
            }
        }

        return Task.CompletedTask;
    }
}