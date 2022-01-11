namespace AzerothMemories.WebBlazor.Common;

public sealed class AddCommentTransferData
{
    [JsonInclude] public string Comment { get; init; }
}