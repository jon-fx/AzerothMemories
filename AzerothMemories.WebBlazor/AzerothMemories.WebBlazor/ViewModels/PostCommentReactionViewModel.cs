namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentReactionViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public long CommentId;

    [JsonInclude] public long AccountId;

    [JsonInclude] public string AccountUsername;

    [JsonInclude] public PostReaction Reaction;

    [JsonInclude] public long LastUpdateTime;
}