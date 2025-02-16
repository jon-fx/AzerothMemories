namespace AzerothMemories.WebBlazor.Blizzard;

public static class BlizzardRealmVersionExt
{
    public static readonly BlizzardRealmVersion[] AllRealmVersions = [BlizzardRealmVersion.Main, BlizzardRealmVersion.Classic, BlizzardRealmVersion.ClassicProgression];

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
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(realmVersion), realmVersion, null);
            }
        }
    }
}