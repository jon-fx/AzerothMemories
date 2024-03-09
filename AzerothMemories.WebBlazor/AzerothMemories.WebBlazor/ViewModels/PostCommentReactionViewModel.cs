namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostCommentReactionViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int CommentId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string AccountUsername { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostReaction Reaction { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long LastUpdateTime { get; init; }
}