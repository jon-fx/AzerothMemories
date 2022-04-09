namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class SpellDataSeeder : GenericBase<SpellDataSeeder>
{
    public SpellDataSeeder(ILogger<SpellDataSeeder> logger, HttpClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("SpellName", "ID", ref data);
        WowTools.Main.LoadDataFromWowTools("SpellMisc", "SpellID", ref data, "engb", new[] { "ID", "SpellID", "SpellIconFileDataID" });

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Spell, reference.Id, reference.GetLocalised("Name_lang"));

            if (reference.TryGetData<int>("SpellIconFileDataID", out var iconId))
            {
                await ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Spell, reference.Id, iconId);
            }
            else
            {
                Logger.LogWarning($"Spell: {reference.Id} - Missing SpellIconFileDataID");
            }
        }
    }
}