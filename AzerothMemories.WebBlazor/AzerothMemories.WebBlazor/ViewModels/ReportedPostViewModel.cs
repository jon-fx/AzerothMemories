namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ReportedPostViewModel
{
    [JsonInclude] public PostViewModel PostViewModel { get; init; }

    [JsonInclude] public List<ReportedChildViewModel> Reports { get; init; } = new();
}