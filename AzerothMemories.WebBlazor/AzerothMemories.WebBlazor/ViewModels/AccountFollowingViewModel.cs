namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountFollowingViewModel
{
    [JsonInclude] public int Id;

    [JsonInclude] public int AccountId;

    [JsonInclude] public int FollowerId;

    [JsonInclude] public string FollowerUsername;

    [JsonInclude] public string FollowerAvatarLink;

    [JsonInclude] public AccountFollowingStatus Status;
}