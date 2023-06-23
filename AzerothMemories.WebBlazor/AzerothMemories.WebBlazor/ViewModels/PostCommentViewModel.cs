namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostCommentViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id;
    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId;
    [JsonInclude, DataMember, MemoryPackInclude] public string AccountAvatar;
    [JsonInclude, DataMember, MemoryPackInclude] public string AccountUsername;

    [JsonInclude, DataMember, MemoryPackInclude] public int PostId;
    [JsonInclude, DataMember, MemoryPackInclude] public int ParentId;

    [JsonInclude, DataMember, MemoryPackInclude] public string PostComment;
    [JsonInclude, DataMember, MemoryPackInclude] public int[] ReactionCounters;
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalReactionCount;

    [JsonInclude, DataMember, MemoryPackInclude] public long CreatedTime;
    [JsonInclude, DataMember, MemoryPackInclude] public long DeletedTimeStamp;

    [JsonInclude, DataMember, MemoryPackInclude] public int CommentPage;
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostCommentViewModel> Children = new();

    public string GetAccountUsernameSafe()
    {
        if (string.IsNullOrWhiteSpace(AccountUsername))
        {
            return "Unknown";
        }

        return AccountUsername;
    }
}