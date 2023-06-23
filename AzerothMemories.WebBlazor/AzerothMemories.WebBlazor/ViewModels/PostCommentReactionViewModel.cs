namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostCommentReactionViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id;

    [JsonInclude, DataMember, MemoryPackInclude] public int CommentId;

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId;

    [JsonInclude, DataMember, MemoryPackInclude] public string AccountUsername;

    [JsonInclude, DataMember, MemoryPackInclude] public PostReaction Reaction;

    [JsonInclude, DataMember, MemoryPackInclude] public long LastUpdateTime;
}