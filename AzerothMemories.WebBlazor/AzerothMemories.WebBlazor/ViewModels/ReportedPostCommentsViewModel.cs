namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class ReportedPostCommentsViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public required PostCommentViewModel CommentViewModel { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public List<ReportedChildViewModel> Reports { get; init; } = [];
}