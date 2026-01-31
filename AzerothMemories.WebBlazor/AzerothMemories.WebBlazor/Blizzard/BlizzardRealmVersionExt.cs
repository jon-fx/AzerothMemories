namespace AzerothMemories.WebBlazor.Blizzard;

public static class BlizzardRealmVersionExt
{
    public static readonly BlizzardRealmVersion[] AllRealmVersions = [BlizzardRealmVersion.Main, BlizzardRealmVersion.Classic, BlizzardRealmVersion.ClassicProgression, BlizzardRealmVersion.ClassicAnniversary];

    public static byte ToValue(this BlizzardRealmVersion blizzardRealmVersion)
    {
        return (byte)blizzardRealmVersion;
    }

    public static string ToUpdateTypePart(this BlizzardRealmVersion realmVersion)
    {
        switch (realmVersion)
        {
            case BlizzardRealmVersion.Main:
            {
                return "";
            }
            case BlizzardRealmVersion.Classic:
            {
                return "_ClassicEra";
            }
            case BlizzardRealmVersion.ClassicProgression:
            {
                return "_ClassicProgression";
            }
            case BlizzardRealmVersion.ClassicAnniversary:
            {
                return "_ClassicAnniversary";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(realmVersion), realmVersion, null);
            }
        }
    }

    public static string GetStaticNamespace(this BlizzardRealmVersion realmVersion)
    {
        switch (realmVersion)
        {
            case BlizzardRealmVersion.Main:
            {
                return "static";
            }
            case BlizzardRealmVersion.Classic:
            {
                return "static-classic1x";
            }
            case BlizzardRealmVersion.ClassicProgression:
            {
                return "static-classic";
            }
            case BlizzardRealmVersion.ClassicAnniversary:
            {
                return "static-classicann";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(realmVersion), realmVersion, null);
            }
        }
    }

    public static string GetDynamicNamespace(this BlizzardRealmVersion realmVersion)
    {
        switch (realmVersion)
        {
            case BlizzardRealmVersion.Main:
            {
                return "dynamic";
            }
            case BlizzardRealmVersion.Classic:
            {
                return "dynamic-classic1x";
            }
            case BlizzardRealmVersion.ClassicProgression:
            {
                return "dynamic-classic";
            }
            case BlizzardRealmVersion.ClassicAnniversary:
            {
                return "dynamic-classicann";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(realmVersion), realmVersion, null);
            }
        }
    }

    public static string GetProfileNamespace(this BlizzardRealmVersion realmVersion)
    {
        switch (realmVersion)
        {
            case BlizzardRealmVersion.Main:
            {
                return "profile";
            }
            case BlizzardRealmVersion.Classic:
            {
                return "profile-classic1x";
            }
            case BlizzardRealmVersion.ClassicProgression:
            {
                return "profile-classic";
            }
            case BlizzardRealmVersion.ClassicAnniversary:
            {
                return "profile-classicann";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(realmVersion), realmVersion, null);
            }
        }
    }

    public static string GetRealmTagSuffix(this BlizzardRealmVersion realmVersion)
    {
        switch (realmVersion)
        {
            case BlizzardRealmVersion.Main:
            {
                return string.Empty;
            }
            case BlizzardRealmVersion.Classic:
            {
                return " (ClassicEra)";
            }
            case BlizzardRealmVersion.ClassicProgression:
            {
                return " (Classic)";
            }
            case BlizzardRealmVersion.ClassicAnniversary:
            {
                return " (Anniversary)";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(realmVersion), realmVersion, null);
            }
        }
    }
}