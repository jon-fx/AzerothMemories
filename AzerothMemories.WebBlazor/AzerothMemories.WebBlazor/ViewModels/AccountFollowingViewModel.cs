namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountFollowingViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public long AccountId;

    [JsonInclude] public long FollowerId;

    [JsonInclude] public string FollowerUsername;

    [JsonInclude] public string FollowerAvatarLink;

    [JsonInclude] public AccountFollowingStatus Status;
}