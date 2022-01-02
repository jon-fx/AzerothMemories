namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class CreatureDataSeeder : GenericBase<CreatureDataSeeder>
{
    public CreatureDataSeeder(ILogger<CreatureDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("creature", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Npc, reference.Id, reference.GetLocalised("Name_lang"));
        }

        return Task.CompletedTask;
    }
}