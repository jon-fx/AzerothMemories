namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class PostViewModel
{
    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] private PostViewModelBlobInfo[] _blobInfo;

    [JsonInclude, DataMember, MemoryPackInclude] public int Id;

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountId;

    [JsonInclude, DataMember, MemoryPackInclude] public string AccountAvatar;

    [JsonInclude, DataMember, MemoryPackInclude] public string AccountUsername;

    [JsonInclude, DataMember, MemoryPackInclude] public string PostAvatar;

    [JsonInclude, DataMember, MemoryPackInclude] public string PostComment;

    [JsonInclude, DataMember, MemoryPackInclude] public byte PostVisibility;

    [JsonInclude, DataMember, MemoryPackInclude] public long PostTime;

    [JsonInclude, DataMember, MemoryPackInclude] public long PostEditedTime;

    [JsonInclude, DataMember, MemoryPackInclude] public long PostCreatedTime;

    [JsonInclude, DataMember, MemoryPackInclude] public string[] ImageBlobNames;

    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo[] SystemTags;

    [JsonInclude, DataMember, MemoryPackInclude] public int ReactionId;

    [JsonInclude, DataMember, MemoryPackInclude] public PostReaction Reaction;

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalCommentCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalReactionCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int[] ReactionCounters;

    [JsonInclude, DataMember, MemoryPackInclude] public long DeletedTimeStamp;

    public PostViewModelBlobInfo[] GetImageBlobInfo()
    {
        return _blobInfo ??= PostViewModelBlobInfo.CreateBlobInfo(AccountUsername, PostComment, ImageBlobNames);
    }
}