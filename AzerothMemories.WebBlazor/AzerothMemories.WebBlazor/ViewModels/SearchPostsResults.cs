namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class SearchPostsResults
{
    [JsonInclude] public int TotalPages { get; set; }
    [JsonInclude] public int CurrentPage { get; set; }

    [JsonInclude] public long MinTime { get; set; }
    [JsonInclude] public long MaxTime { get; set; }
    [JsonInclude] public PostSortMode SortMode { get; set; }

    [JsonInclude] public PostTagInfo[] Tags { get; set; } = Array.Empty<PostTagInfo>();
    [JsonInclude] public PostViewModel[] PostViewModels = Array.Empty<PostViewModel>();
}