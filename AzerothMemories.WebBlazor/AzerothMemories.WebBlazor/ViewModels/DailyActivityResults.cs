namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial record DailyActivityResults
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Year { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public DailyActivityResultsMain Main { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public DailyActivityResultsUser User { get; init; }
}