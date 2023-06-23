namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AccountFollowingViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id;

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId;

    [JsonInclude, DataMember, MemoryPackInclude] public int FollowerId;

    [JsonInclude, DataMember, MemoryPackInclude] public string FollowerUsername;

    [JsonInclude, DataMember, MemoryPackInclude] public string FollowerAvatarLink;

    [JsonInclude, DataMember, MemoryPackInclude] public AccountFollowingStatus Status;
}