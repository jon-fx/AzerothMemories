namespace AzerothMemories.WebBlazor.Common;

[DataContract, MemoryPackable]
public sealed partial class AddCommentTransferData
{
    [JsonInclude, DataMember, MemoryPackInclude] public string Comment { get; init; }
}