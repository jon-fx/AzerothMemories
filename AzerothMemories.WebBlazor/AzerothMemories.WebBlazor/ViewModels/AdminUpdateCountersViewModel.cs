namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AdminUpdateCountersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int NoneCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int QueuedCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int DoneCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int NewEventCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int ProcessedEventCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int DiscardedEventCount { get; init; }
}