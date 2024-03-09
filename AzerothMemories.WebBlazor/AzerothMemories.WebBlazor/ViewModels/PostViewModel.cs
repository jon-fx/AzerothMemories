namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostViewModel
{
    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] private PostViewModelBlobInfo[] _blobInfo;

    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string AccountAvatar { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string AccountUsername { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string PostAvatar { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public string PostComment { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public byte PostVisibility { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public long PostTime { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long PostEditedTime { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long PostCreatedTime { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string[] ImageBlobNames { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo[] SystemTags { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int ReactionId { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostReaction Reaction { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalCommentCount { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalReactionCount { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int[] ReactionCounters { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long DeletedTimeStamp { get; set; }

    public PostViewModelBlobInfo[] GetImageBlobInfo()
    {
        return _blobInfo ??= PostViewModelBlobInfo.CreateBlobInfo(AccountUsername, PostComment, ImageBlobNames);
    }
}