namespace AzerothMemories.WebBlazor.Services;

public static class PostTagInfoAdd
{
    public static bool IsRetailOnlyTag(this PostTagType tagType)
    {
        switch (tagType)
        {
            case PostTagType.Realm:
            case PostTagType.Character:
            case PostTagType.Guild:
            {
                return true;
            }
            case PostTagType.None:
            case PostTagType.Type:
            case PostTagType.Main:
            case PostTagType.Region:
            case PostTagType.Account:
            case PostTagType.Achievement:
            case PostTagType.Item:
            case PostTagType.Mount:
            case PostTagType.Pet:
            case PostTagType.Zone:
            case PostTagType.Npc:
            case PostTagType.Spell:
            case PostTagType.Object:
            case PostTagType.Quest:
            case PostTagType.ItemSet:
            case PostTagType.Toy:
            case PostTagType.Title:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            case PostTagType.HashTag:
            default:
            {
                return false;
            }
        }
    }

    public static bool CanBeReported(this PostTagType tagType)
    {
        switch (tagType)
        {
            case PostTagType.None:
            case PostTagType.HashTag:
            {
                return false;
            }
            case PostTagType.Type:
            case PostTagType.Main:
            case PostTagType.Region:
            case PostTagType.Realm:
            case PostTagType.Account:
            case PostTagType.Character:
            case PostTagType.Guild:
            case PostTagType.Achievement:
            case PostTagType.Item:
            case PostTagType.Mount:
            case PostTagType.Pet:
            case PostTagType.Zone:
            case PostTagType.Npc:
            case PostTagType.Spell:
            case PostTagType.Object:
            case PostTagType.Quest:
            case PostTagType.ItemSet:
            case PostTagType.Toy:
            case PostTagType.Title:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            {
                return true;
            }
            default:
            {
                return false;
            }
        }
    }

    public static string GetTagIcon(this PostTagType tagType)
    {
        switch (tagType)
        {
            case PostTagType.Zone:
            {
                return "🏰 ";
            }
            case PostTagType.Region:
            {
                return "🌌 ";
            }
            case PostTagType.Realm:
            {
                return "🪐 ";
            }
            case PostTagType.Account:
            {
                return "🔶 ";
            }
            case PostTagType.Character:
            {
                return "🔷 ";
            }
            case PostTagType.Guild:
            {
                return "🔺 ";
            }
            case PostTagType.HashTag:
            {
                return "🏁 ";
            }
            case PostTagType.None:
            case PostTagType.Type:
            case PostTagType.Main:
            case PostTagType.Mount:
            case PostTagType.Pet:
            case PostTagType.Item:
            case PostTagType.Achievement:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            case PostTagType.Npc:
            case PostTagType.Object:
            case PostTagType.Spell:
            case PostTagType.Quest:
            case PostTagType.ItemSet:
            case PostTagType.Toy:
            case PostTagType.Title:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return "";
    }

    public static string GetWowHeadLink(this PostTagType tagType, long id)
    {
        switch (tagType)
        {
            case PostTagType.None:
            case PostTagType.Type:
            case PostTagType.Main:
            {
                break;
            }
            case PostTagType.Mount:
            {
                return $"mount={id}";
            }
            case PostTagType.Pet:
            {
                return $"battle-pet={id}";
            }
            case PostTagType.Item:
            {
                return $"item={id}";
            }
            case PostTagType.Achievement:
            {
                return $"achievement={id}";
            }
            case PostTagType.Zone:
            {
                return $"zone={id}";
            }
            case PostTagType.Npc:
            {
                return $"npc={id}";
            }
            case PostTagType.Spell:
            {
                return $"spell={id}";
            }
            case PostTagType.Object:
            {
                return $"object={id}";
            }
            case PostTagType.Quest:
            {
                return $"quest={id}";
            }
            case PostTagType.ItemSet:
            {
                return $"itemset={id}";
            }
            case PostTagType.Toy:
            case PostTagType.Title:
            case PostTagType.Region:
            case PostTagType.Realm:
            case PostTagType.Account:
            case PostTagType.Character:
            case PostTagType.Guild:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            case PostTagType.HashTag:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return null;
    }

    public static Color GetChipColor(this PostTagInfo tagInfo)
    {
        return Color.Primary;
    }
}