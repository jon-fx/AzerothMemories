namespace AzerothMemories.WebBlazor.Components;

public sealed class AddMemoryComponentResult
{
    [JsonInclude] public readonly long AccountId;
    [JsonInclude] public readonly long PostId;
    [JsonInclude] public readonly AddMemoryResult Result;
    [JsonInclude] public readonly AddMemoryTransferData Data;

    public AddMemoryComponentResult(AddMemoryTransferData data, AddMemoryResult result, long accountId, long postId)
    {
        Data = data;
        Result = result;
        AccountId = accountId;
        PostId = postId;
    }
}