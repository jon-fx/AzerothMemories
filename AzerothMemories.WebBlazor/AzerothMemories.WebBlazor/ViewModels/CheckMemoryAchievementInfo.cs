namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CheckMemoryAchievementInfo
{
    [JsonInclude, DataMember, MemoryPackInclude] public long TimeStamp { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo? Achievement { get; set; }
}