namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class RealmDataSeeder : GenericBase<RealmDataSeeder>
{
    public RealmDataSeeder(ILogger<RealmDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var regions = new[] { BlizzardRegion.Europe, BlizzardRegion.UnitedStates, BlizzardRegion.Taiwan, BlizzardRegion.Korea /* BlizzardRegionId.China */};

        foreach (var region in regions)
        {
            using var client = WarcraftClientProvider.Get(region);
            var twoLetters = region.ToInfo().TwoLetters.ToUpper();

            var allRealmSearchResults = await ResourceCache.GetOrRequestData($"RealmData-{region.ToInfo().TwoLetters}", async k => await client.GetRealmData());
            if (allRealmSearchResults != null)
            {
                foreach (var realmData in allRealmSearchResults.Realms)
                {
                    var realmRecord = realmData.Name.ToRecord();
                    SetExtensions.Update(realmRecord, (l, x) => $"{twoLetters}-{x}");

                    ResourceWriter.AddServerSideLocalizationName(PostTagType.Realm, realmData.Id, realmRecord);
                    ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Realm, realmData.Id, realmData.Slug);

                    //ResourceWriter.AddClientSideCommonLocalizationData($"RealmSlug-{realmData.Id}", realmData.Slug);
                }
            }

            var connectedRealmData = await ResourceCache.GetOrRequestData($"RealmConnectedData-{region.ToInfo().TwoLetters}", async k => await client.GetConnectedRealmData());
            if (connectedRealmData != null)
            {
            }
        }
    }
}