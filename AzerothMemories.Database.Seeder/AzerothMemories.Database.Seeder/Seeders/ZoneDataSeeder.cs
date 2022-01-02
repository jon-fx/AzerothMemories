namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class ZoneDataSeeder : GenericBase<ZoneDataSeeder>
{
    public ZoneDataSeeder(ILogger<ZoneDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("areatable", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Zone, reference.Id, reference.GetLocalised("AreaName_lang"));
        }

        return Task.CompletedTask;
    }
}