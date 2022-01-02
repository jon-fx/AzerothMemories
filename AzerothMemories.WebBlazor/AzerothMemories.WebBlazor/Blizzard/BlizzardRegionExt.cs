namespace AzerothMemories.WebBlazor.Blizzard;

public static class BlizzardRegionExt
{
    public static BlizzardRegionInfo ToInfo(this BlizzardRegion region)
    {
        return BlizzardRegionInfo.AllById[(int)region];
    }

    public static byte ToValue(this BlizzardRegion blizzardRegion)
    {
        return (byte)blizzardRegion;
    }

    public static BlizzardRegion FromName(string name)
    {
        return BlizzardRegionInfo.AllByName[name].Region;
    }
}