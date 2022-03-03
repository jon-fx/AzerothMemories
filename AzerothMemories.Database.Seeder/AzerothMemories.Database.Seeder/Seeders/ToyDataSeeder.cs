namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class ToyDataSeeder : GenericBase<ToyDataSeeder>
{
    public ToyDataSeeder(ILogger<ToyDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("Toy", "ID", ref data, new[] { "ID", "ItemID" });

        foreach (var reference in data.Values)
        {
            if (reference.TryGetData<int>("ItemID", out var itemId))
            {
                var dataToCopy = ResourceWriter.GetOrCreateServerSideResource(PostTagType.Item, itemId);

                ResourceWriter.AddServerSideLocalizationName(PostTagType.Toy, reference.Id, dataToCopy.Name);
                ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Toy, reference.Id, dataToCopy.Media);
            }
        }

        return Task.CompletedTask;
    }
}