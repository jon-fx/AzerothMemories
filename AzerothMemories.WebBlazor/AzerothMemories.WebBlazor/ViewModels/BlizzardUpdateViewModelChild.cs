namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class BlizzardUpdateViewModelChild
{
    [JsonInclude, DataMember, MemoryPackInclude] public long Id;

    [JsonInclude, DataMember, MemoryPackInclude] public byte UpdateType { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public string UpdateTypeString { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public HttpStatusCode UpdateJobLastResult;
}