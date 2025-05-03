namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CheckMemoryResult
{
    [JsonInclude, DataMember, MemoryPackInclude] public PostViewModel[] CurrentPosts { get; set; } = [];
    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo[] Achievements { get; set; } = [];
    [JsonInclude, DataMember, MemoryPackInclude] public int MatchingAchievements { get; set; }
}