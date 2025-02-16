namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CharacterViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? Ref { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardRegion RegionId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int RealmId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardRealmVersion RealmVersion { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string Name { get; init; } = null!;

    [JsonInclude, DataMember, MemoryPackInclude] public byte Class { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public byte Race { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public byte Gender { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public byte Level { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public CharacterStatus2 CharacterStatus { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public bool AccountSync { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? AvatarLink { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? GuildRef { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? GuildName { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModel? UpdateJobLastResults { get; init; }

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool IsLoadingFromArmory => UpdateJobLastResults == null || UpdateJobLastResults.IsLoadingFromArmory || Class == 0 || RealmId == 0;

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public string TagString => PostTagInfo.GetTagString(PostTagType.Character, Id);
}