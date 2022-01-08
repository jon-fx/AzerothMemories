namespace AzerothMemories.WebBlazor.Services;

public sealed class AddCommentTransferData
{
    [JsonInclude] public string Comment { get; init; }
}