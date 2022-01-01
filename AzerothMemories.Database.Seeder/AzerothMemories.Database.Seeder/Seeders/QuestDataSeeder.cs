namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class QuestDataSeeder : GenericBase<QuestDataSeeder>
{
    public QuestDataSeeder(ILogger<QuestDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("QuestV2CliTask", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddLocalizationData($"QuestName-{reference.Id}", reference.GetLocalised("QuestTitle_lang"));
        }

        return Task.CompletedTask;
    }
}