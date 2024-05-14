namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class ReportedPostTagsViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public required PostViewModel PostViewModel { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public List<ReportedChildViewModel> Reports { get; init; } = [];
}