namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class PetDataSeeder : GenericBase<PetDataSeeder>
{
    public PetDataSeeder(ILogger<PetDataSeeder> logger, HttpClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("BattlePetSpecies", "ID", ref data, new[] { "ID", "SummonSpellID", "IconFileDataID", "CreatureID" });

        foreach (var reference in data.Values)
        {
            BlizzardDataRecord dataToCopy = null; 
            if (reference.TryGetData<int>("SummonSpellID", out var spellId) && ResourceWriter.TryGetServerSideResource(PostTagType.Spell, spellId, out dataToCopy))
            {
            }

            if (dataToCopy == null && reference.TryGetData<int>("CreatureID", out var creatureId) && ResourceWriter.TryGetServerSideResource(PostTagType.Npc, creatureId, out dataToCopy))
            {
            }

            if (dataToCopy == null)
            {
            }
            else
            {
                ResourceWriter.AddServerSideLocalizationName(PostTagType.Pet, reference.Id, dataToCopy.Name);
            }

            if (reference.TryGetData<int>("IconFileDataID", out var iconId))
            {
                await ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Pet, reference.Id, iconId);
            }
        }
    }
}