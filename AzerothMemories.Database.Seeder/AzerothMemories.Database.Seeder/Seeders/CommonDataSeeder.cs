namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class CommonDataSeeder : GenericBase<CommonDataSeeder>
{
    public CommonDataSeeder(ILogger<CommonDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    //protected override string FileName => "Common.json";

    protected override Task DoSomething()
    {
        ResourceWriter.AddCommonLocalizationData("RegionName-0", "None");
        ResourceWriter.AddCommonLocalizationData($"RegionName-{(int)BlizzardRegion.Europe}", BlizzardRegion.Europe.ToInfo().Name);
        ResourceWriter.AddCommonLocalizationData($"RegionName-{(int)BlizzardRegion.UnitedStates}", BlizzardRegion.UnitedStates.ToInfo().Name);
        ResourceWriter.AddCommonLocalizationData($"RegionName-{(int)BlizzardRegion.Taiwan}", BlizzardRegion.Taiwan.ToInfo().Name);
        ResourceWriter.AddCommonLocalizationData($"RegionName-{(int)BlizzardRegion.China}", BlizzardRegion.China.ToInfo().Name);
        ResourceWriter.AddCommonLocalizationData($"RegionName-{(int)BlizzardRegion.Korea}", BlizzardRegion.Korea.ToInfo().Name);

        ResourceWriter.AddCommonLocalizationData("TypeName-0", "Retail");
        ResourceWriter.AddCommonLocalizationData("TypeName-1", "Classic");
        ResourceWriter.AddCommonLocalizationData("TypeName-2", "Season of Mastery");

        ResourceWriter.AddCommonLocalizationData("MainName-0", "None");
        ResourceWriter.AddCommonLocalizationData("MainName-1", "Dungeons");
        ResourceWriter.AddCommonLocalizationData("MainName-2", "Raids");
        ResourceWriter.AddCommonLocalizationData("MainName-3", "Arena");
        ResourceWriter.AddCommonLocalizationData("MainName-4", "Battlegrounds");
        ResourceWriter.AddCommonLocalizationData("MainName-5", "PvP");
        ResourceWriter.AddCommonLocalizationData("MainName-6", "Achievements");
        ResourceWriter.AddCommonLocalizationData("MainName-7", "Collections");
        ResourceWriter.AddCommonLocalizationData("MainName-8", "Mounts");
        ResourceWriter.AddCommonLocalizationData("MainName-9", "Pets");

        return Task.CompletedTask;
    }
}