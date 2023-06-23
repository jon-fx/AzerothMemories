namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostCommentPageViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Page;
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalPages;
    [JsonInclude, DataMember, MemoryPackInclude] public Dictionary<int, PostCommentViewModel> AllComments = new();
}