﻿namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class RealmDataSeeder : GenericBase<RealmDataSeeder>
{
    public RealmDataSeeder(ILogger<RealmDataSeeder> logger, HttpClientProvider clientProvider, WowTools wowTools, MoaResourceWriter resourceWriter) : base(logger, clientProvider, wowTools, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var regions = new[] { BlizzardRegion.Europe, BlizzardRegion.UnitedStates, BlizzardRegion.Taiwan, BlizzardRegion.Korea /* BlizzardRegionId.China */};

        foreach (var region in regions)
        {
            using var client = HttpClientProvider.GetWarcraftClient(region);
            var twoLetters = region.ToInfo().TwoLettersUpper;

            var allRealmSearchResults = await ResourceCache.GetOrRequestData($"RealmData-{region.ToInfo().TwoLettersUpper}", async k => await client.GetRealmData());
            if (allRealmSearchResults != null)
            {
                foreach (var realmData in allRealmSearchResults.Realms)
                {
                    var realmRecord = realmData.Name.ToArray();
                    SetExtensions.Update(realmRecord, (l, x) => $"{twoLetters}-{x}");

                    ResourceWriter.AddServerSideLocalizationName(PostTagType.Realm, realmData.Id, realmRecord);
                    ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Realm, realmData.Id, realmData.Slug);

                    //ResourceWriter.AddClientSideCommonLocalizationData($"RealmSlug-{realmData.Id}", realmData.Slug);
                }
            }

            var connectedRealmData = await ResourceCache.GetOrRequestData($"RealmConnectedData-{region.ToInfo().TwoLettersUpper}", async k => await client.GetConnectedRealmData());
            if (connectedRealmData != null)
            {
            }
        }
    }
}