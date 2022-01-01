namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class ItemDataSeeder : GenericBase<ItemDataSeeder>
{
    public ItemDataSeeder(ILogger<ItemDataSeeder> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(logger, clientProvider, resourceCache, resourceWriter)
    {
    }

    protected override Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();

        WowTools.LoadDataFromWowTools("Item", "ID", ref data, "engb", new[] { "IconFileDataID" });
        WowTools.LoadDataFromWowTools("ItemSparse", "ID", ref data, new[] { "Display_lang" });
        WowTools.LoadDataFromWowTools("ItemSearchName", "ID", ref data, new[] { "Display_lang", "OverallQualityID" });
        WowTools.LoadDataFromWowTools("ItemModifiedAppearance", "ItemID", ref data, "engb", new[] { "ItemAppearanceID" });
        WowTools.LoadDataFromWowTools("ItemXItemEffect", "ItemID", ref data, "engb", new[] { "ItemEffectID" });

        var appearance = new Dictionary<int, WowToolsData>();
        WowTools.LoadDataFromWowTools("ItemAppearance", "ID", ref appearance, "engb", new[] { "DefaultIconFileDataID" });

        //var itemEffects = new Dictionary<int, WowToolsData>();
        //WowTools.LoadDataFromWowTools("ItemEffect", "ID", ref itemEffects, "engb", new[] { "SpellID" });

        foreach (var reference in data.Values)
        {
            if (reference.TryGetData<int>("OverallQualityID", out var quality) && (quality == 0 || quality == 1))
            {
                continue;
            }

            var displayLangName = reference.GetLocalised("Display_lang");
            if (displayLangName.IsNull())
            {
                //if (reference.TryGetData<int>("ItemEffectID", out var itemEffectId) && itemEffects.TryGetValue(itemEffectId, out var itemEffectWowData))
                //{
                //    if (itemEffectWowData.TryGetData<int>("SpellID", out var spellId))
                //    {
                //    }
                //}
            }
            else
            {
                ResourceWriter.AddLocalizationData($"ItemName-{reference.Id}", displayLangName);
            }

            if (reference.TryGetData<int>("IconFileDataID", out var iconId))
            {
                if (iconId == 0)
                {
                    if (reference.TryGetData<int>("ItemAppearanceID", out var itemAppearanceId) && appearance.TryGetValue(itemAppearanceId, out var appearanceWowData))
                    {
                        if (appearanceWowData.TryGetData("DefaultIconFileDataID", out iconId))
                        {
                        }
                    }
                }

                if (iconId > 0 && WowTools.TryGetIconName(iconId, out var iconName))
                {
                    var newValue = $"https://render.worldofwarcraft.com/eu/icons/56/{iconName}.jpg";
                    ResourceWriter.AddCommonLocalizationData($"ItemIconMediaPath-{reference.Id}", newValue);
                }
                else
                {
                    Logger.LogWarning($"Item: {reference.Id} - Missing Icon: {iconId}");
                }
            }
        }

        return Task.CompletedTask;
    }
}