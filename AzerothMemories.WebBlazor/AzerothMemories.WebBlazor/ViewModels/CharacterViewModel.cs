namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CharacterViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id;

    [JsonInclude, DataMember, MemoryPackInclude] public string Ref;

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardRegion RegionId;

    [JsonInclude, DataMember, MemoryPackInclude] public int RealmId;

    [JsonInclude, DataMember, MemoryPackInclude] public string Name;

    [JsonInclude, DataMember, MemoryPackInclude] public byte Class;

    [JsonInclude, DataMember, MemoryPackInclude] public byte Race;

    [JsonInclude, DataMember, MemoryPackInclude] public byte Gender;

    [JsonInclude, DataMember, MemoryPackInclude] public byte Level;

    [JsonInclude, DataMember, MemoryPackInclude] public CharacterStatus2 CharacterStatus;

    [JsonInclude, DataMember, MemoryPackInclude] public bool AccountSync;

    [JsonInclude, DataMember, MemoryPackInclude] public string AvatarLink;

    [JsonInclude, DataMember, MemoryPackInclude] public string GuildRef;

    [JsonInclude, DataMember, MemoryPackInclude] public string GuildName;

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModel UpdateJobLastResults;

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public string AvatarLinkWithFallBack => GetAvatarStringWithFallBack(AvatarLink, Race, Gender);

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool IsLoadingFromArmory => UpdateJobLastResults == null || UpdateJobLastResults.IsLoadingFromArmory || Class == 0 || RealmId == 0;

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public string TagString => PostTagInfo.GetTagString(PostTagType.Character, Id);

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