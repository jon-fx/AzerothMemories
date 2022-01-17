namespace AzerothMemories.WebBlazor.Common;

public enum AddMemoryResultCode
{
    None,
    Success,
    Canceled,
    Failed,
    UploadFailed,
    CommentTooLong,
    ParseCommentFailed,
    SessionNotFound,
    InvalidTime,
    InvalidTags,
    TooManyTags
}