namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostReactionViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public long AccountId;

    [JsonInclude] public PostReaction Reaction;

    [JsonInclude] public string AccountUsername;
}