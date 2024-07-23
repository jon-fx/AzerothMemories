namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class RecentPostsResults
{
    [JsonInclude, DataMember, MemoryPackInclude] public int CurrentPage { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostSortMode SortMode { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public RecentPostType PostType { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostInfo[] PostInfos { get; init; } = [];

    [JsonInclude, DataMember, MemoryPackInclude] public PostViewModel?[] PostViewModels { get; set; } = [];
}