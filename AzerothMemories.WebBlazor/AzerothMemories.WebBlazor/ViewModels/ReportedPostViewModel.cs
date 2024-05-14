namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class ReportedPostViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public required PostViewModel PostViewModel { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public List<ReportedChildViewModel> Reports { get; init; } = [];
}