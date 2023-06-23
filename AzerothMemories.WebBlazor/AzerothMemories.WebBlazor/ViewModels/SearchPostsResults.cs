namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class SearchPostsResults
{
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalPages { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public int CurrentPage { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public long MinTime { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public long MaxTime { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public PostSortMode SortMode { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo[] Tags { get; set; } = Array.Empty<PostTagInfo>();
    [JsonInclude, DataMember, MemoryPackInclude] public PostViewModel[] PostViewModels = Array.Empty<PostViewModel>();
}