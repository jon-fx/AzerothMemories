namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ReportedPostCommentsViewModel
{
    [JsonInclude] public PostCommentViewModel CommentViewModel { get; init; }

    [JsonInclude] public List<ReportedChildViewModel> Reports { get; init; } = new();
}