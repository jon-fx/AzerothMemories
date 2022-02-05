namespace AzerothMemories.WebBlazor.Common;

public sealed class AddMemoryResult
{
    [JsonInclude] public readonly long AccountId;
    [JsonInclude] public readonly long PostId;
    [JsonInclude] public readonly AddMemoryResultCode Result;

    public AddMemoryResult(AddMemoryResultCode result, long accountId = 0, long postId = 0)
    {
        Result = result;
        AccountId = accountId;
        PostId = postId;
    }
}