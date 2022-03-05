namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostViewModel
{
    [JsonIgnore] private PostViewModelBlobInfo[] _blobInfo;

    public PostViewModel()
    {
    }

    [JsonInclude] public int Id;

    [JsonInclude] public int AccountId;

    [JsonInclude] public string AccountAvatar;

    [JsonInclude] public string AccountUsername;

    [JsonInclude] public string PostAvatar;

    [JsonInclude] public string PostComment;

    [JsonInclude] public byte PostVisibility;

    [JsonInclude] public long PostTime;

    [JsonInclude] public long PostEditedTime;

    [JsonInclude] public long PostCreatedTime;

    [JsonInclude] public string[] ImageBlobNames;

    [JsonInclude] public PostTagInfo[] SystemTags;

    [JsonInclude] public int ReactionId;

    [JsonInclude] public PostReaction Reaction;

    [JsonInclude] public int TotalCommentCount;

    [JsonInclude] public int TotalReactionCount;

    [JsonInclude] public int[] ReactionCounters;

    [JsonInclude] public long DeletedTimeStamp;

    public PostViewModelBlobInfo[] GetImageBlobInfo()
    {
        return _blobInfo ??= PostViewModelBlobInfo.CreateBlobInfo(AccountUsername, PostComment, ImageBlobNames);
    }
}