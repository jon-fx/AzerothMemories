namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivitySetUser
{
    public int Year { get; init; }

    public Instant StartTime { get; init; }

    public Instant EndTime { get; init; }

    public HashSet<int> Achievements { get; init; } = new();

    public HashSet<int> FirstAchievements { get; init; } = new();

    public HashSet<DailyActivityResultsUserPostInfo> MyMemories { get; init; } = new();
}