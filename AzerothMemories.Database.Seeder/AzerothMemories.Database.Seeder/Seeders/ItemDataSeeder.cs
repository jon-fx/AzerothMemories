﻿namespace AzerothMemories.Database.Seeder.Seeders;

internal sealed class ItemDataSeeder : GenericBase<ItemDataSeeder>
{
    public ItemDataSeeder(ILogger<ItemDataSeeder> logger, HttpClientProvider clientProvider, WowTools wowTools, MoaResourceWriter resourceWriter) : base(logger, clientProvider, wowTools, resourceWriter)
    {
    }

    protected override async Task DoSomething()
    {
        var data = new Dictionary<int, WowToolsData>();

        WowTools.Main.LoadDataFromWowTools("Item", "ID", ref data, "engb", new[] { "IconFileDataID" });
        WowTools.Main.LoadDataFromWowTools("ItemSparse", "ID", ref data, new[] { "Display_lang" });
        WowTools.Main.LoadDataFromWowTools("ItemSearchName", "ID", ref data, new[] { "Display_lang", "OverallQualityID" });
        WowTools.Main.LoadDataFromWowTools("ItemModifiedAppearance", "ItemID", ref data, "engb", new[] { "ItemAppearanceID" });
        WowTools.Main.LoadDataFromWowTools("ItemXItemEffect", "ItemID", ref data, "engb", new[] { "ItemEffectID" });

        var appearance = new Dictionary<int, WowToolsData>();
        WowTools.Main.LoadDataFromWowTools("ItemAppearance", "ID", ref appearance, "engb", new[] { "DefaultIconFileDataID" });

        foreach (var reference in data.Values)
        {
            if (reference.TryGetData<int>("OverallQualityID", out var quality) && (quality == 0 || quality == 1))
            {
                continue;
            }

            var displayLangName = reference.GetLocalised("Display_lang");
            if (reference.HasLocalised("Display_lang"))
            {
                ResourceWriter.AddServerSideLocalizationName(PostTagType.Item, reference.Id, displayLangName);
            }
            else
            {
                //if (reference.TryGetData<int>("ItemEffectID", out var itemEffectId) && itemEffects.TryGetValue(itemEffectId, out var itemEffectWowData))
                //{
                //    if (itemEffectWowData.TryGetData<int>("SpellID", out var spellId))
                //    {
                //    }
                //}
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

                await ResourceWriter.TryAddServerSideLocalizationMedia(PostTagType.Item, reference.Id, iconId);
            }
        }
    }
}