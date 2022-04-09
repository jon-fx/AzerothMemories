namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class MountDataSeeder : GenericBase<MountDataSeeder>
{
    public MountDataSeeder(ILogger<MountDataSeeder> logger, HttpClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("Mount", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Mount, reference.Id, reference.GetLocalised("Name_lang"));

            if (reference.TryGetData<int>("SourceSpellID", out var sourceSpellId))
            {
                var spellResource = ResourceWriter.GetOrCreateServerSideResource(PostTagType.Spell, sourceSpellId);
                if (spellResource?.Media != null)
                {
                    ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Mount, reference.Id, spellResource.Media);
                }
                else
                {
                    Logger.LogWarning($"Mount: {reference.Id} - Missing Resource SpellIconMediaPath:{sourceSpellId}");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return Task.CompletedTask;
    }
}