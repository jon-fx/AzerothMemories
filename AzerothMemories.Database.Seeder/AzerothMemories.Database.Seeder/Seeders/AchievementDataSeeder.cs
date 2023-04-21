namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class AchievementDataSeeder : GenericBase<AchievementDataSeeder>
{
    public AchievementDataSeeder(ILogger<AchievementDataSeeder> logger, HttpClientProvider clientProvider, WowTools wowTools, MoaResourceWriter resourceWriter) : base(logger, clientProvider, wowTools, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("achievement", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddServerSideLocalizationName(PostTagType.Achievement, reference.Id, reference.GetLocalised("Title_lang"));

            if (reference.TryGetData<int>("IconFileID", out var iconId))
            {
                await ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Achievement, reference.Id, iconId);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}