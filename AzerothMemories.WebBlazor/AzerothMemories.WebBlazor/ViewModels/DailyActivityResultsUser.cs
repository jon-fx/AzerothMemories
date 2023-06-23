namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial record DailyActivityResultsUser
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Year { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public string ZoneId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public long StartTimeMs { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public long EndTimeMs { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostTagInfo> Achievements { get; init; } = new();
    [JsonInclude, DataMember, MemoryPackInclude] public List<PostTagInfo> FirstAchievements { get; init; } = new();
    [JsonInclude, DataMember, MemoryPackInclude] public List<DailyActivityResultsUserPostInfo> MyMemories { get; init; } = new();
}