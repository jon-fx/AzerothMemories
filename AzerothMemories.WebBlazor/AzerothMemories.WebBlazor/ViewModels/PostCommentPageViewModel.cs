namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostCommentPageViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Page { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalPages { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public Dictionary<int, PostCommentViewModel> AllComments { get; init; } = new();
}