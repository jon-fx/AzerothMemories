namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AccountFollowingViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int FollowerId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string FollowerUsername { get; init; } = string.Empty;

    [JsonInclude, DataMember, MemoryPackInclude] public string? FollowerAvatarLink { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public AccountFollowingStatus Status { get; set; }
}