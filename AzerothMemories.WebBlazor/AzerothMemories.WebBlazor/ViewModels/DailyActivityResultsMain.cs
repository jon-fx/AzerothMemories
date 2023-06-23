namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial record DailyActivityResultsMain
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Year { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public string ZoneId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public long StartTimeMs { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public long EndTimeMs { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalTags { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalAchievements { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostTagInfo> TopTags { get; init; } = new();
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostTagInfo> TopAchievements { get; init; } = new();
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostTagInfo> FirstTags { get; init; } = new();
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostTagInfo> FirstAchievements { get; init; } = new();
}