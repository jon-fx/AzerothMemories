namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class GuildViewModel
{
    [JsonInclude] public int Id;

    [JsonInclude] public string Avatar;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public int RealmId;

    [JsonInclude] public string Name;

    [JsonInclude] public int MemberCount;

    [JsonInclude] public int AchievementPoints;

    [JsonInclude] public long CreatedDateTime;

    [JsonInclude] public long BlizzardCreatedTimestamp;

    [JsonInclude] public BlizzardUpdateViewModel UpdateJobLastResults;

    [JsonInclude] public GuildMembersViewModel MembersViewModel;

    [JsonIgnore] public bool IsLoadingFromArmory => UpdateJobLastResults == null || UpdateJobLastResults.IsLoadingFromArmory || RealmId == 0;

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
}