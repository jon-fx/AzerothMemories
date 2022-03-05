namespace AzerothMemories.Database.Seeder.Base;

public static class ClientExtensions
{
    public static Task<RequestResult<PlayableRacesIndex>> GetPlayableRaceIndex(this WarcraftClient client)
    {
        return client.Get<PlayableRacesIndex>(BlizzardNamespace.Static, "/data/wow/playable-race/index ", null, null, true, null);
    }

    public static Task<RequestResult<PlayableRace>> GetPlayableRace(this WarcraftClient client, int id)
    {
        return client.Get<PlayableRace>(BlizzardNamespace.Static, $"/data/wow/playable-race/{id}", null, null, true, null);
    }

    public static Task<RequestResult<PlayableClassesIndex>> GetPlayableClassIndex(this WarcraftClient client)
    {
        return client.Get<PlayableClassesIndex>(BlizzardNamespace.Static, "/data/wow/playable-class/index ", null, null, true, null);
    }

    public static Task<RequestResult<PlayableClass>> GetPlayableClass(this WarcraftClient client, int id)
    {
        return client.Get<PlayableClass>(BlizzardNamespace.Static, $"/data/wow/playable-class/{id}", null, null, true, null);
    }

    public static Task<RequestResult<PlayableClassMedia>> GetPlayableClassMedia(this WarcraftClient client, int id)
    {
        return client.Get<PlayableClassMedia>(BlizzardNamespace.Static, $"/data/wow/media/playable-class/{id}", null, null, true, null);
    }

    public static Task<RequestResult<PlayableSpecializationMedia>> GetPlayableSpecializationClassMedia(this WarcraftClient client, int id)
    {
        return client.Get<PlayableSpecializationMedia>(BlizzardNamespace.Static, $"/data/wow/media/playable-specialization/{id}", null, null, true, null);
    }

    public static Task<RequestResult<RealmsIndex>> GetRealmData(this WarcraftClient client)
    {
        return client.Get<RealmsIndex>(BlizzardNamespace.Dynamic, "/data/wow/realm/index", null, null, true, null);
    }

    public static Task<RequestResult<ConnectedRealmsIndex>> GetConnectedRealmData(this WarcraftClient client)
    {
        return client.Get<ConnectedRealmsIndex>(BlizzardNamespace.Dynamic, "/data/wow/connected-realm", null, null, true, null);
    }

    //public static Task<RequestResult<AchievementsIndex>> GetAchievementsIndex(this WarcraftClient client)
    //{
    //    return client.Get<AchievementsIndex>(BlizzardNamespace.Static, "/data/wow/achievement/index", null, null, true, -1);
    //}

    //public static Task<RequestResult<Achievement>> GetAchievementsData(this WarcraftClient client, int id)
    //{
    //    return client.Get<Achievement>(BlizzardNamespace.Static, $"/data/wow/achievement/{id}", null, null, true, -1);
    //}

    //public static Task<RequestResult<AchievementMedia>> GetAchievementsMedia(this WarcraftClient client, int id)
    //{
    //    return client.Get<AchievementMedia>(BlizzardNamespace.Static, $"/data/wow/media/achievement/{id}", null, null, true, -1);
    //}

    //public static Task<RequestResult<MountsIndex>> GetMounts(this WarcraftClient client)
    //{
    //    return client.Get<MountsIndex>(BlizzardNamespace.Static, "/data/wow/mount/index", null, null, true, -1);
    //}

    //public static Task<RequestResult<Mount>> GetMountInfo(this WarcraftClient client, int id)
    //{
    //    return client.Get<Mount>(BlizzardNamespace.Static, $"/data/wow/mount/{id}", null, null, true, -1);
    //}

    //public static Task<RequestResult<PetsIndex>> GetPets(this WarcraftClient client)
    //{
    //    return client.Get<PetsIndex>(BlizzardNamespace.Static, "/data/wow/pet/index", null, null, true, -1);
    //}

    //public static Task<RequestResult<Pet>> GetPetInfo(this WarcraftClient client, int id)
    //{
    //    return client.Get<Pet>(BlizzardNamespace.Static, $"/data/wow/pet/{id}", null, null, true, -1);
    //}

    //public static Task<RequestResult<CreatureDisplayMedia>> GetCreatureDisplayMedia(this WarcraftClient client, int id)
    //{
    //    return client.Get<CreatureDisplayMedia>(BlizzardNamespace.Static, $"/data/wow/media/creature-display/{id}", null, null, true, -1);
    //}

    //public static Task<RequestResult<Item>> GetItemInfo(this WarcraftClient client, int id)
    //{
    //    return client.Get<Item>(BlizzardNamespace.Static, $"/data/wow/item/{id}", null, null, true, -1);
    //}

    //public static Task<RequestResult<ItemMedia>> GetItemMedia(this WarcraftClient client, int id)
    //{
    //    return client.Get<ItemMedia>(BlizzardNamespace.Static, $"/data/wow/media/item/{id}", null, null, true, -1);
    //}

    //public static Task<RequestResult<JournalExpansionsIndex>> GetJournalIndex(this WarcraftClient client)
    //{
    //    return client.Get<JournalExpansionsIndex>(BlizzardNamespace.Static, "/data/wow/journal-expansion/index", null, null, true, -1);
    //}

    //public static Task<RequestResult<JournalExpansion>> GetJournalIndex(this WarcraftClient client, int tierId)
    //{
    //    return client.Get<JournalExpansion>(BlizzardNamespace.Static, $"/data/wow/journal-expansion/{tierId} ", null, null, true, -1);
    //}

    //public static Task<RequestResult<JournalEncountersIndex>> GetJournalEncountersIndex(this WarcraftClient client)
    //{
    //    return client.Get<JournalEncountersIndex>(BlizzardNamespace.Static, "/data/wow/journal-encounter/index ", null, null, true, -1);
    //}

    //public static Task<RequestResult<Encounter>> GetJournalEncountersIndex(this WarcraftClient client, int referenceId)
    //{
    //    return client.Get<Encounter>(BlizzardNamespace.Static, $"/data/wow/journal-encounter/{referenceId}  ", null, null, true, -1);
    //}
}