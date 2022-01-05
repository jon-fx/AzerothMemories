namespace AzerothMemories.WebBlazor.ViewModels;

public class PostViewModel
{
    public long Id;

    public long AccountId;

    public string AccountAvatar;

    public string AccountUsername;

    public string PostAvatar;

    public string PostComment;

    public byte PostVisibility;

    public DateTimeOffset PostTime;

    public DateTimeOffset PostEditedTime;

    public DateTimeOffset PostCreatedTime;

    public string[] ImageBlobNames;

    public string SystemTags;

    public PostTagInfo[] SystemTagsArray;

    public long ReactionId;

    public PostReaction Reaction;

    public int TotalCommentCount;

    public int TotalReactionCount;

    public int[] ReactionCounters;

    public long DeletedTimeStamp;

    //public PostReactionViewModel[] ReactionData;
}