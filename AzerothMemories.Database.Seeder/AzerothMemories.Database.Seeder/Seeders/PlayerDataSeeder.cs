namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class PlayerDataSeeder : GenericBase<PlayerDataSeeder>
{
    public PlayerDataSeeder(ILogger<PlayerDataSeeder> logger, HttpClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var characterClasses = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("ChrClasses", "ID", ref characterClasses);

        foreach (var reference in characterClasses.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterClass, reference.Id, reference.GetLocalised("Name_lang"));

            if (reference.TryGetData<int>("IconFileDataID", out var iconId))
            {
                if (iconId == 0)
                {
                }
                else
                {
                    await ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.CharacterClass, reference.Id, iconId);
                }
            }
        }

        var characterSpecs = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("ChrSpecialization", "ID", ref characterSpecs);

        foreach (var reference in characterSpecs.Values)
        {
            if (!reference.TryGetData<int>("ClassId", out var classId))
            {
                continue;
            }

            if (classId <= 0)
            {
                continue;
            }

            ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterClassSpecialization, reference.Id, reference.GetLocalised("Name_lang"));

            if (reference.TryGetData<int>("IconFileDataID", out var iconId))
            {
                if (iconId == 0)
                {
                }
                else
                {
                    await ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.CharacterClassSpecialization, reference.Id, iconId);
                }
            }

            var specRecord = ResourceWriter.GetOrCreateServerSideResource(PostTagType.CharacterClassSpecialization, reference.Id);
            SetExtensions.Update(specRecord.Names, (l, x) =>
            {
                if (ResourceWriter.TryGetServerSideResource(PostTagType.CharacterClass, classId, out var dataRecord) && dataRecord.TryGetNameOrDefault(l, out var characterClass))
                {
                    return $"{x} ({characterClass})";
                }

                return x;
            });
        }

        var characterRaces = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("ChrRaces", "ID", ref characterRaces);

        foreach (var reference in characterRaces.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterRace, reference.Id, reference.GetLocalised("Name_lang"));
        }
    }
}