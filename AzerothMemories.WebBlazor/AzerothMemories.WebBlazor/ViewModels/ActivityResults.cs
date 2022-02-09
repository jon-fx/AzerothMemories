namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record ActivityResults
{
    [JsonInclude] public int Status { get; init; }
    [JsonInclude] public ActivityResultsChild Totals { get; init; } = new();
    [JsonInclude] public List<ActivityResultsChild> DataByYear { get; init; } = new();
}