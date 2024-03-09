namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostCommentViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public string AccountAvatar { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public string AccountUsername { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int PostId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int ParentId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string PostComment { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int[] ReactionCounters { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalReactionCount { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long CreatedTime { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public long DeletedTimeStamp { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int CommentPage { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostCommentViewModel> Children { get; init; } = new();

    public string GetAccountUsernameSafe()
    {
        if (string.IsNullOrWhiteSpace(AccountUsername))
        {
            return "Unknown";
        }

        return AccountUsername;
    }
}