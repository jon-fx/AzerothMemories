namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class MountDataSeeder : GenericBase<MountDataSeeder>
{
    public MountDataSeeder(ILogger<MountDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("Mount", "ID", ref data);

        foreach (var reference in data.Values)
        {
            ResourceWriter.AddLocalizationData($"MountName-{reference.Id}", reference.GetLocalised("Name_lang"));

            if (reference.TryGetData<int>("SourceSpellID", out var sourceSpellId))
            {
                if (ResourceWriter.GetLocalizationData(BlizzardLocale.None, $"SpellIconMediaPath-{sourceSpellId}", out var spellResourceResult))
                {
                    ResourceWriter.AddCommonLocalizationData($"MountIconMediaPath-{reference.Id}", spellResourceResult);
                }
                else
                {
                    Logger.LogWarning($"Mount: {reference.Id} - Missing Resource SpellIconMediaPath:{sourceSpellId}");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return Task.CompletedTask;
    }
}