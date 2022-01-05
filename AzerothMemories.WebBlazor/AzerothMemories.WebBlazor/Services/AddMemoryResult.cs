namespace AzerothMemories.WebBlazor.Services
{
    public enum AddMemoryResult
    {
        None,
        Success,
        Canceled,
        Failed,
        CommentTooLong,
        ParseCommentFailed,
        SessionNotFound,
        InvalidTime,
        InvalidTags,
        TagsTooLong
    }
}