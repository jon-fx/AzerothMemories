namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class RecentPostsResults
{
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalPages { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public int CurrentPage { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public PostSortMode SortMode { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public RecentPostsType PostsType { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostViewModel[] PostViewModels = Array.Empty<PostViewModel>();
}