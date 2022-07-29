namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class CommonDataSeeder : GenericBase<CommonDataSeeder>
{
    public CommonDataSeeder(ILogger<CommonDataSeeder> logger, HttpClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Region, (int)BlizzardRegion.None, GetFilledLocal("None"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Region, (int)BlizzardRegion.Europe, GetFilledLocal(BlizzardRegion.Europe.ToInfo().Name));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Region, (int)BlizzardRegion.UnitedStates, GetFilledLocal(BlizzardRegion.UnitedStates.ToInfo().Name));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Region, (int)BlizzardRegion.Taiwan, GetFilledLocal(BlizzardRegion.Taiwan.ToInfo().Name));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Region, (int)BlizzardRegion.China, GetFilledLocal(BlizzardRegion.China.ToInfo().Name));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Region, (int)BlizzardRegion.Korea, GetFilledLocal(BlizzardRegion.Korea.ToInfo().Name));

        ResourceWriter.AddServerSideLocalizationName(PostTagType.Type, 0, GetFilledLocal("Retail"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Type, 1, GetFilledLocal("Classic"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Type, 2, GetFilledLocal("Season of Mastery"));

        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 0, GetFilledLocal("None"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 1, GetFilledLocal("Dungeons"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 2, GetFilledLocal("Raids"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 3, GetFilledLocal("Arena"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 4, GetFilledLocal("Battlegrounds"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 5, GetFilledLocal("PvP"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 6, GetFilledLocal("Achievements"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 7, GetFilledLocal("Collections"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 8, GetFilledLocal("Mounts"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.Main, 9, GetFilledLocal("Pets"));

        ResourceWriter.AddServerSideLocalizationName(PostTagType.Realm, 0, GetFilledLocal("Unknown Realm"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterClass, 0, GetFilledLocal("Unknown Class"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterClassSpecialization, 0, GetFilledLocal("Unknown Specialization"));
        ResourceWriter.AddServerSideLocalizationName(PostTagType.CharacterRace, 0, GetFilledLocal("Unknown Race"));

        return Task.CompletedTask;
    }

    private string[] GetFilledLocal(string value)
    {
        var result = new string[(int)ServerSideLocale.Count];
        result[(int)ServerSideLocale.En_Us] = value;

        return result;
    }
}