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

    [JsonInclude] public long UpdateJobEndTime;

    [JsonInclude] public CharacterViewModel[] CharactersArray;

    [JsonInclude] public HttpStatusCode UpdateJobLastResult;

    [JsonIgnore] public bool IsLoadingFromArmory => UpdateJobLastResult == 0 || UpdateJobEndTime == 0 || RealmId == 0;

    public string GetPageTitle()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return "Memories of Azeroth";
        }

        return $"{Name}'s Memories of Azeroth";
    }

    public string GetAvatarText()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return "?";
        }

        return Name[0].ToString();
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