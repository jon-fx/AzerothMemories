namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentViewModel
{
    [JsonInclude] public int Id;
    [JsonInclude] public int AccountId;
    [JsonInclude] public string AccountAvatar;
    [JsonInclude] public string AccountUsername;

    [JsonInclude] public int PostId;
    [JsonInclude] public int ParentId;

    [JsonInclude] public string PostComment;
    [JsonInclude] public int[] ReactionCounters;
    [JsonInclude] public int TotalReactionCount;

    [JsonInclude] public long CreatedTime;
    [JsonInclude] public long DeletedTimeStamp;

    [JsonInclude] public int CommentPage;
    [JsonInclude] public List<PostCommentViewModel> Children = new();

    public string GetAccountUsernameSafe()
    {
        if (string.IsNullOrWhiteSpace(AccountUsername))
        {
            return "Unknown";
        }

        return AccountUsername;
    }
}