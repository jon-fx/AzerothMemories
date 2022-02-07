namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivityResults
{
    [JsonInclude] public ActivityResultsChild Totals { get; set; } = new();
    [JsonInclude] public List<ActivityResultsChild> DataByYear { get; set; } = new();
}