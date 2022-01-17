namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class GuildViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public int RealmId;

    [JsonInclude] public string Name;

    [JsonInclude] public int MemberCount;

    [JsonInclude] public int AchievementPoints;

    [JsonInclude] public long CreatedDateTime;

    [JsonInclude] public long BlizzardCreatedTimestamp;

    [JsonIgnore] public string AvatarLinkWithFallBack;

    [JsonInclude] public CharacterViewModel[] Characters;
}