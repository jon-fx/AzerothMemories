namespace AzerothMemories.WebBlazor.Common;

public sealed class AddMemoryResult
{
    [JsonInclude] public readonly int AccountId;
    [JsonInclude] public readonly int PostId;
    [JsonInclude] public readonly AddMemoryResultCode Result;

    public AddMemoryResult(AddMemoryResultCode result, int accountId = 0, int postId = 0)
    {
        Result = result;
        AccountId = accountId;
        PostId = postId;
    }
}