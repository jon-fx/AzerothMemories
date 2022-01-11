namespace AzerothMemories.WebBlazor.Common;

public enum AddMemoryResultCode
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
    TooManyTags
}