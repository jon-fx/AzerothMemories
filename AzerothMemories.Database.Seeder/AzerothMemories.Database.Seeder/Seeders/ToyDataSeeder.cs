namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class ToyDataSeeder : GenericBase<ToyDataSeeder>
{
    public ToyDataSeeder(ILogger<ToyDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("Toy", "ID", ref data, new[] { "ID", "ItemID" });

        foreach (var reference in data.Values)
        {
            if (reference.TryGetData<int>("ItemID", out var itemId))
            {
                ResourceWriter.CloneLocalizationData($"ItemName-{itemId}", $"ToyName-{reference.Id}");
                ResourceWriter.CloneLocalizationData($"ItemIconMediaPath-{itemId}", $"ToyIconMediaPath-{reference.Id}");
            }
        }

        return Task.CompletedTask;
    }
}