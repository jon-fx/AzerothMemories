namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class QuestDataSeeder : GenericBase<QuestDataSeeder>
{
    public QuestDataSeeder(ILogger<QuestDataSeeder> logger, HttpClientProvider clientProvider, WowTools wowTools, MoaResourceWriter resourceWriter) : base(logger, clientProvider, wowTools, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("QuestV2CliTask", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Quest, reference.Id, reference.GetLocalised("QuestTitle_lang"));
        }

        return Task.CompletedTask;
    }
}