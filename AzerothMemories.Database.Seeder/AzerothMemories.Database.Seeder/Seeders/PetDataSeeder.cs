﻿namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class PetDataSeeder : GenericBase<PetDataSeeder>
{
    public PetDataSeeder(ILogger<PetDataSeeder> logger, HttpClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("BattlePetSpecies", "ID", ref data, new[] { "ID", "SummonSpellID", "IconFileDataID" });

        foreach (var reference in data.Values)
        {
            if (reference.TryGetData<int>("SummonSpellID", out var spellId))
            {
                var dataToCopy = ResourceWriter.GetOrCreateServerSideResource(PostTagType.Spell, spellId);
                ResourceWriter.AddServerSideLocalizationName(PostTagType.Pet, reference.Id, dataToCopy.Name);
            }

            if (reference.TryGetData<int>("IconFileDataID", out var iconId))
            {
                await ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Pet, reference.Id, iconId);
            }
        }
    }
}