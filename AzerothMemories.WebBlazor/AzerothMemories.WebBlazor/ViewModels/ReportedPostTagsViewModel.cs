namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ReportedPostTagsViewModel
{
    [JsonInclude] public PostViewModel PostViewModel { get; init; }

    [JsonInclude] public List<ReportedChildViewModel> Reports { get; init; } = new();
}