namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentPageViewModel
{
    [JsonInclude] public int Page;
    [JsonInclude] public int TotalPages;
    [JsonInclude] public Dictionary<int, PostCommentViewModel> AllComments = new();
}