namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AdminUpdateCountersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int NoneCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int QueuedCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int ProgressCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int RequiredCount { get; init; }
}