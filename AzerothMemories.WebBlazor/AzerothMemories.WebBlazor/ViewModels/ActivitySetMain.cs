namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivitySetMain
{
    public int Year { get; init; }

    public Instant StartTime { get; init; }

    public Instant EndTime { get; init; }

    public int TotalAchievements { get; set; }

    public HashSet<string> FirstTags { get; } = new();

    public HashSet<int> FirstAchievements { get; } = new();

    public Dictionary<int, int> AchievementCounts { get; } = new();

    public Dictionary<string, int> PostTags { get; } = new();

    public bool IsEmpty()
    {
        return FirstTags.Count == 0 && FirstAchievements.Count == 0 && AchievementCounts.Count == 0 && PostTags.Count == 0;
    }
}