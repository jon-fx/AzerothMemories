namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AdminUpdateCountersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int NoneCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int QueuedCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int ProgressCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int RequiredCount;
}