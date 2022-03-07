namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class CharacterViewModel
{
    [JsonInclude] public int Id;

    [JsonInclude] public string Ref;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public int RealmId;

    [JsonInclude] public string Name;

    [JsonInclude] public byte Class;

    [JsonInclude] public byte Race;

    [JsonInclude] public byte Gender;

    [JsonInclude] public byte Level;

    [JsonInclude] public CharacterStatus2 CharacterStatus;

    //[JsonInclude] public CharacterFaction Faction;

    //[JsonInclude] public int AchievementTotalPoints;

    //[JsonInclude] public int AchievementTotalQuantity;

    [JsonInclude] public bool AccountSync;

    [JsonInclude] public string AvatarLink;

    [JsonInclude] public string GuildRef;

    [JsonInclude] public string GuildName;

    //[JsonInclude] public byte GuildRank;

    [JsonInclude] public long UpdateJobLastEndTime;

    [JsonInclude] public HttpStatusCode UpdateJobLastResult;

    [JsonIgnore] public string AvatarLinkWithFallBack => GetAvatarStringWithFallBack(AvatarLink, Race, Gender);

    [JsonIgnore] public bool IsLoadingFromArmory => UpdateJobLastResult == 0 || UpdateJobLastEndTime == 0 || Class == 0 || RealmId == 0;

    [JsonIgnore] public string TagString => PostTagInfo.GetTagString(PostTagType.Character, Id);

    public string GetPageTitle()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return "Memories of Azeroth";
        }

        return $"{Name}'s Memories of Azeroth";
    }

    public static string GetAvatarStringWithFallBack(string avatarLink, byte race, byte gender)
    {
        if (string.IsNullOrEmpty(avatarLink))
        {
            avatarLink = "https://render-us.worldofwarcraft.com/character/tichondrius/00/000000000-avatar.jpg";
        }

        return $"{avatarLink}?alt=/shadow/avatar/{race}-{gender}.jpg";
    }
}