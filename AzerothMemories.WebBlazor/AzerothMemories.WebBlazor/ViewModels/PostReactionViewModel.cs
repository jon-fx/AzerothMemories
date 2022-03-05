namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostReactionViewModel
{
    [JsonInclude] public int Id;

    [JsonInclude] public int AccountId;

    [JsonInclude] public string AccountUsername;

    [JsonInclude] public string AccountAvatar;

    [JsonInclude] public PostReaction Reaction;

    [JsonInclude] public long LastUpdateTime;
}