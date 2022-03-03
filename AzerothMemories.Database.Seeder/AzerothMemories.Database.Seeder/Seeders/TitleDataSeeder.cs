namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class TitleDataSeeder : GenericBase<TitleDataSeeder>
{
    public TitleDataSeeder(ILogger<TitleDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("CharTitles", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Title, reference.Id, reference.GetLocalised("Name_lang"));
        }

        return Task.CompletedTask;
    }
}