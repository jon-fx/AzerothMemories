namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class PlayerDataSeeder : GenericBase<PlayerDataSeeder>
{
    public PlayerDataSeeder(ILogger<PlayerDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        using var client = WarcraftClientProvider.Get(BlizzardRegion.Europe);
        var classIndex = await ResourceCache.GetOrRequestData("ClassIndex", async k => await client.GetPlayableClassIndex());
        if (classIndex != null)
        {
            foreach (var reference in classIndex.Classes)
            {
                var classInfo = await ResourceCache.GetOrRequestData($"Class-{reference.Id}", async k => await client.GetPlayableClass(reference.Id));
                if (classInfo != null)
                {
                    ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterClass, classInfo.Id, classInfo.Name.ToRecord());

                    var classMedia = await ResourceCache.GetOrRequestData($"Class-{classInfo.Id}-Media", async k => await client.GetPlayableClassMedia(classInfo.Id));
                    if (classMedia != null)
                    {
                        var media = classMedia.Assets.FirstOrDefault(x => x.Key == "icon");
                        if (media != null)
                        {
                            ResourceWriter.AddServerSideLocalizationMedia(PostTagType.CharacterClass, classInfo.Id, media.Value.AbsoluteUri);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    foreach (var dataSpecialization in classInfo.Specializations)
                    {
                        var specRecord = dataSpecialization.Name.ToRecord();

                        SetExtensions.Update(specRecord, (l, x) =>
                        {
                            if (ResourceWriter.GetClientSideLocalizationData(l, $"CharacterClass-{classInfo.Id}", out var classNameString))
                            {
                                return $"{x} ({classNameString})";
                            }

                            return x;
                        });

                        ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterClassSpecialization, dataSpecialization.Id, specRecord);

                        var specMedia = await ResourceCache.GetOrRequestData($"ClassSpecialization-{dataSpecialization.Id}-Media", async k => await client.GetPlayableSpecializationClassMedia(dataSpecialization.Id));
                        if (specMedia != null)
                        {
                            var media = specMedia.Assets.FirstOrDefault(x => x.Key == "icon");
                            if (media != null)
                            {
                                ResourceWriter.AddServerSideLocalizationMedia(PostTagType.CharacterClassSpecialization, dataSpecialization.Id, media.Value.AbsoluteUri);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
            }
        }

        var raceIndex = await ResourceCache.GetOrRequestData("RaceIndex", async k => await client.GetPlayableRaceIndex());
        if (raceIndex != null)
        {
            foreach (var reference in raceIndex.Races)
            {
                var raceInfo = await ResourceCache.GetOrRequestData($"Race-{reference.Id}", async k => await client.GetPlayableRace(reference.Id));
                if (raceInfo != null)
                {
                    ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterRace, raceInfo.Id, raceInfo.Name.ToRecord());
                }
            }
        }
    }
}