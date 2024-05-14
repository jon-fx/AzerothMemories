namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class GuildViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? Avatar { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardRegion RegionId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int RealmId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string Name { get; init; } = null!;

    [JsonInclude, DataMember, MemoryPackInclude] public string MoaRef { get; init; } = null!;

    [JsonInclude, DataMember, MemoryPackInclude] public int MemberCount { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int AchievementPoints { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long CreatedDateTime { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long BlizzardCreatedTimestamp { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModel? UpdateJobLastResults { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public GuildMembersViewModel? MembersViewModel { get; init; }

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool IsLoadingFromArmory => UpdateJobLastResults == null || UpdateJobLastResults.IsLoadingFromArmory || RealmId == 0;
}