namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class PetDataSeeder : GenericBase<PetDataSeeder>
{
    public PetDataSeeder(ILogger<PetDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("BattlePetSpecies", "ID", ref data, new[] { "ID", "SummonSpellID", "IconFileDataID" });

        foreach (var reference in data.Values)
        {
            if (reference.TryGetData<int>("SummonSpellID", out var spellId))
            {
                var dataToCopy = ResourceWriter.GetOrCreateServerSideResource(PostTagType.Spell, spellId);
                ResourceWriter.AddServerSideLocalizationName(PostTagType.Pet, reference.Id, dataToCopy.Name);
            }

            if (reference.TryGetData<int>("IconFileDataID", out var iconId))
            {
                if (iconId > 0 && WowTools.TryGetIconName(iconId, out var iconName))
                {
                    var newValue = $"https://render.worldofwarcraft.com/eu/icons/56/{iconName}.jpg";
                    ResourceWriter.AddServerSideLocalizationMedia(PostTagType.Pet, reference.Id, newValue);
                }
                else
                {
                    Logger.LogWarning($"Pet: {reference.Id} - Missing Icon: {iconId}");
                }
            }
        }

        return Task.CompletedTask;
    }
}