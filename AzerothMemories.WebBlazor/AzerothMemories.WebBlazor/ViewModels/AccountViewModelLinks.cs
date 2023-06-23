namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AccountViewModelLinks
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public string Name { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public string Key { get; set; }
}