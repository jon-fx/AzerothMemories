namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class RecentPostsResults
{
    [JsonInclude] public int TotalPages { get; set; }
    [JsonInclude] public int CurrentPage { get; set; }
    [JsonInclude] public PostSortMode SortMode { get; set; }
    [JsonInclude] public RecentPostsType PostsType { get; set; }

    [JsonInclude] public PostViewModel[] PostViewModels = Array.Empty<PostViewModel>();
}