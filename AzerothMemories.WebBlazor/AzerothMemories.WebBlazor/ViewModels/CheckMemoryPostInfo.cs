namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CheckMemoryPostInfo
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public long PostTime { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo[] SystemTags { get; set; } = [];
}