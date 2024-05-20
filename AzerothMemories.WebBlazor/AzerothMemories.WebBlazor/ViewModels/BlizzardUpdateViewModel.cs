namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class BlizzardUpdateViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long UpdateLastModified { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long UpdateJobLastEndTime { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public HttpStatusCode UpdateJobLastResult { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModelChild[] Children { get; init; } = [];

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool IsLoadingFromArmory => UpdateJobLastEndTime == 0;
}