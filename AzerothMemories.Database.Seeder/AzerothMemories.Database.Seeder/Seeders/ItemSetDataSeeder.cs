namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class ItemSetDataSeeder : GenericBase<ItemSetDataSeeder>
{
    public ItemSetDataSeeder(ILogger<ItemSetDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("ItemSet", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.ItemSet, reference.Id, reference.GetLocalised("Name_lang"));
        }

        return Task.CompletedTask;
    }
}