namespace AzerothMemories.WebBlazor.ViewModels;

public sealed partial class ActivitySetMain
{
    public int Year { get; init; }

    public Instant StartTime { get; init; }

    public Instant EndTime { get; init; }

    public int TotalAchievements { get; set; }

    public HashSet<string> FirstTags { get; set; } = new();

    public HashSet<int> FirstAchievements { get; set; } = new();

    public Dictionary<int, int> AchievementCounts { get; set; } = new();

    public Dictionary<string, int> PostTags { get; set; } = new();
}