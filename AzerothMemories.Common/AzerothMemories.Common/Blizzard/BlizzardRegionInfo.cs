namespace AzerothMemories.Blizzard;

public sealed class BlizzardRegionInfo
{
    public static readonly BlizzardRegionInfo[] AllById = new BlizzardRegionInfo[(int)BlizzardRegion.Count];
    public static readonly Dictionary<string, BlizzardRegionInfo> AllByName = new();

    public static readonly BlizzardRegionInfo China = new(BlizzardRegion.China, "cn")
    {
        Host = "https://gateway.battlenet.com.cn",
        Authorization = "https://www.battlenet.com.cn"
    };

    public static readonly BlizzardRegionInfo Europe = new(BlizzardRegion.Europe, "eu");
    public static readonly BlizzardRegionInfo Korea = new(BlizzardRegion.Korea, "kr") { Authorization = "https://apac.battle.net" };
    public static readonly BlizzardRegionInfo Taiwan = new(BlizzardRegion.Taiwan, "tw") { Authorization = "https://apac.battle.net" };
    public static readonly BlizzardRegionInfo UnitedStates = new(BlizzardRegion.UnitedStates, "us");

    private BlizzardRegionInfo(BlizzardRegion region, string twoLetters)
    {
        Name = region.ToString();
        Region = region;
        TwoLetters = twoLetters;
        Host = $"https://{twoLetters}.api.blizzard.com";
        Authorization = $"https://{twoLetters}.battle.net";

        AllById[(int)region] = this;
        AllByName[Name] = this;
    }

    public string Name { get; }

    public BlizzardRegion Region { get; }

    public string Host { get; private init; }

    public string TwoLetters { get; }

    public string Authorization { get; private init; }

    public string TokenEndpoint => $"{Authorization}/oauth/token";

    public string AuthorizationEndpoint => $"{Authorization}/oauth/authorize";

    public string UserInformationEndpoint => $"{Authorization}/oauth/userinfo";
}