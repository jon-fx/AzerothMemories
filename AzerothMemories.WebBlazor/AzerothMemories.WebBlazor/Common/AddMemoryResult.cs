namespace AzerothMemories.WebBlazor.Common;

[DataContract, MemoryPackable]
public sealed partial class AddMemoryResult
{
    [JsonInclude, DataMember, MemoryPackInclude] public readonly int AccountId;
    [JsonInclude, DataMember, MemoryPackInclude] public readonly int PostId;
    [JsonInclude, DataMember, MemoryPackInclude] public readonly AddMemoryResultCode Result;

    public AddMemoryResult(AddMemoryResultCode result, int accountId = 0, int postId = 0)
    {
        Result = result;
        AccountId = accountId;
        PostId = postId;
    }
}