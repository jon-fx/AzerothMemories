namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentReactionViewModel
{
    [JsonInclude] public int Id;

    [JsonInclude] public int CommentId;

    [JsonInclude] public int AccountId;

    [JsonInclude] public string AccountUsername;

    [JsonInclude] public PostReaction Reaction;

    [JsonInclude] public long LastUpdateTime;
}