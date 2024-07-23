namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostInfoEx : PostInfo
{
    public PostInfoEx(int postId, int accountId, byte postVisibility) : base(postId, accountId)
    {
        PostVisibility = postVisibility;
    }

    [JsonInclude, DataMember, MemoryPackInclude] public byte PostVisibility { get; init; }
}