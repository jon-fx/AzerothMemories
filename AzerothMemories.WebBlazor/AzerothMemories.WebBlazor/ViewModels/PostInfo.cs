namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostInfo
{
    [MemoryPackConstructor]
    public PostInfo(int postId, int accountId, byte postVisibility)
    {
        PostId = postId;
        AccountId = accountId;
        PostVisibility = postVisibility;
    }

    [JsonInclude, DataMember, MemoryPackInclude] public int PostId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public byte PostVisibility { get; init; }
}