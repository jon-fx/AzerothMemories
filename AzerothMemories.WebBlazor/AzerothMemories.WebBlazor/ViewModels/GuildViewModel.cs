namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class GuildViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id;

    [JsonInclude, DataMember, MemoryPackInclude] public string Avatar;

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardRegion RegionId;

    [JsonInclude, DataMember, MemoryPackInclude] public int RealmId;

    [JsonInclude, DataMember, MemoryPackInclude] public string Name;

    [JsonInclude, DataMember, MemoryPackInclude] public int MemberCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int AchievementPoints;

    [JsonInclude, DataMember, MemoryPackInclude] public long CreatedDateTime;

    [JsonInclude, DataMember, MemoryPackInclude] public long BlizzardCreatedTimestamp;

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModel UpdateJobLastResults;

    [JsonInclude, DataMember, MemoryPackInclude] public GuildMembersViewModel MembersViewModel;

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool IsLoadingFromArmory => UpdateJobLastResults == null || UpdateJobLastResults.IsLoadingFromArmory || RealmId == 0;

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