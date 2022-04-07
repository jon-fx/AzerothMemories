namespace AzerothMemories.WebBlazor.Common;

public enum AddMemoryResultCode
{
    None,
    Success,
    Canceled,
    Failed,
    UploadFailed,
    UploadFailedQuota,
    UploadFailedHash,
    UploadFailedHashCheck,
    CommentTooLong,
    ParseCommentFailed,
    SessionNotFound,
    SessionCanNotInteract,
    InvalidTime,
    InvalidTags,
    TooManyTags,
    NoImageMustContainText,
}