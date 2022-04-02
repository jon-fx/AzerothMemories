namespace AzerothMemories.WebBlazor.Blizzard;

public sealed class BlizzardRegionInfo
{
    public static readonly BlizzardRegionInfo[] AllById = new BlizzardRegionInfo[(int)BlizzardRegion.Count];
    public static readonly Dictionary<string, BlizzardRegionInfo> AllByName = new();

    public static readonly BlizzardRegionInfo China = new(BlizzardRegion.China, "cn", "CN")
    {
        Host = "https://gateway.battlenet.com.cn",
        Authorization = "https://www.battlenet.com.cn"
    };

    public static readonly BlizzardRegionInfo Europe = new(BlizzardRegion.Europe, "eu", "EU");
    public static readonly BlizzardRegionInfo Korea = new(BlizzardRegion.Korea, "kr", "KR") { Authorization = "https://apac.battle.net" };
    public static readonly BlizzardRegionInfo Taiwan = new(BlizzardRegion.Taiwan, "tw", "TW") { Authorization = "https://apac.battle.net" };
    public static readonly BlizzardRegionInfo UnitedStates = new(BlizzardRegion.UnitedStates, "us", "US");

    private BlizzardRegionInfo(BlizzardRegion region, string twoLettersLower, string twoLettersUpper)
    {
        Name = region.ToString();
        Region = region;
        TwoLettersLower = twoLettersLower;
        TwoLettersUpper = twoLettersUpper;
        Host = $"https://{twoLettersLower}.api.blizzard.com";
        Authorization = $"https://{twoLettersLower}.battle.net";

        AllById[(int)region] = this;
        AllByName[Name] = this;

        Exceptions.ThrowIf(!twoLettersLower.Equals(twoLettersUpper, StringComparison.OrdinalIgnoreCase));
    }

    public string Name { get; }

    public BlizzardRegion Region { get; }

    public string Host { get; private init; }

    public string TwoLettersLower { get; }

    public string TwoLettersUpper { get; }

    public string Authorization { get; private init; }

    public string TokenEndpoint => $"{Authorization}/oauth/token";

    public string AuthorizationEndpoint => $"{Authorization}/oauth/authorize";

    public string UserInformationEndpoint => $"{Authorization}/oauth/userinfo";
}