namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class CreatureDataSeeder : GenericBase<CreatureDataSeeder>
{
    public CreatureDataSeeder(ILogger<CreatureDataSeeder> logger, HttpClientProvider clientProvider, WowTools wowTools, MoaResourceWriter resourceWriter) : base(logger, clientProvider, wowTools, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("creature", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Npc, reference.Id, reference.GetLocalised("Name_lang"));
        }

        return Task.CompletedTask;
    }
}