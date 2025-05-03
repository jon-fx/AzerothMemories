namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CheckMemoryResult
{
    [JsonInclude, DataMember, MemoryPackInclude] public PostViewModel[] Posts { get; set; } = [];
    [JsonInclude, DataMember, MemoryPackInclude] public CheckMemoryAchievementInfo[] Achievements { get; set; } = [];
}