namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class GuildViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public string Avatar;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public int RealmId;

    [JsonInclude] public string Name;

    [JsonInclude] public int MemberCount;

    [JsonInclude] public int AchievementPoints;

    [JsonInclude] public long CreatedDateTime;

    [JsonInclude] public long BlizzardCreatedTimestamp;

    [JsonInclude] public CharacterViewModel[] CharactersArray;

    public string GetAvatarText()
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            return Name[0].ToString();
        }

        return "?";
    }

    public CharacterViewModel[] GetCharactersSafe()
    {
        if (CharactersArray == null || CharactersArray.Length == 0)
        {
            return Array.Empty<CharacterViewModel>();
        }

        return CharactersArray;
    }
}