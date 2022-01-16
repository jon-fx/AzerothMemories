namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentPageViewModel
{
    [JsonInclude] public int Page;
    [JsonInclude] public int TotalPages;
    [JsonInclude] public Dictionary<long, PostCommentViewModel> AllComments = new();

    [JsonIgnore] public List<PostCommentTreeNode> RootComments = new();
    [JsonIgnore] public Dictionary<long, PostCommentTreeNode> AllCommentTreeNodes = new();
}