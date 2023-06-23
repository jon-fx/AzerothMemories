namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class BlizzardUpdateViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public long UpdateLastModified;

    [JsonInclude, DataMember, MemoryPackInclude] public long UpdateJobLastEndTime;

    [JsonInclude, DataMember, MemoryPackInclude] public HttpStatusCode UpdateJobLastResult;

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModelChild[] Children;

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool IsLoadingFromArmory => UpdateJobLastEndTime == 0;
}