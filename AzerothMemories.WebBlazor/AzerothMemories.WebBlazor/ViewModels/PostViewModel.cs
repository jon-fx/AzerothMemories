namespace AzerothMemories.WebBlazor.ViewModels;

public class PostViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public long AccountId;

    [JsonInclude] public string AccountAvatar;

    [JsonInclude] public string AccountUsername;

    [JsonInclude] public string PostAvatar;

    [JsonInclude] public string PostComment;

    [JsonInclude] public byte PostVisibility;

    [JsonInclude] public long PostTime;

    [JsonInclude] public long PostEditedTime;

    [JsonInclude] public long PostCreatedTime;

    [JsonInclude] public string[] ImageBlobNames;

    [JsonInclude] public string SystemTags;

    //public PostTagInfo[] SystemTagsArray;

    [JsonInclude] public long ReactionId;

    [JsonInclude] public PostReaction Reaction;

    [JsonInclude] public int TotalCommentCount;

    [JsonInclude] public int TotalReactionCount;

    [JsonInclude] public int[] ReactionCounters;

    [JsonInclude] public long DeletedTimeStamp;

    //public PostReactionViewModel[] ReactionData;
}