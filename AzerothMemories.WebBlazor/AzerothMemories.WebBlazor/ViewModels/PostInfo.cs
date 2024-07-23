namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public partial class PostInfo
{
    [MemoryPackConstructor]
    public PostInfo(int postId, int accountId)
    {
        PostId = postId;
        AccountId = accountId;
    }

    [JsonInclude, DataMember, MemoryPackInclude] public int PostId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; init; }
}