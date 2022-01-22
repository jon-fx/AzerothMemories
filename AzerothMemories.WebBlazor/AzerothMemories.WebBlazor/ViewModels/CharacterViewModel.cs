namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class CharacterViewModel
{
    [JsonInclude] public long Id;

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

    //[JsonInclude] public long LastUpdateJobEndTime;

    [JsonInclude] public HttpStatusCode LastUpdateHttpResult;

    [JsonIgnore]
    public string AvatarLinkWithFallBack
    {
        get
        {
            var str = AvatarLink;
            if (string.IsNullOrEmpty(str))
            {
                str = "https://render-us.worldofwarcraft.com/character/tichondrius/00/000000000-avatar.jpg";
            }

            return $"{str}?alt=/shadow/avatar/{Race}-{Gender}.jpg";
        }
    }

    [JsonIgnore] public string TagString => PostTagInfo.GetTagString(PostTagType.Character, Id);
}