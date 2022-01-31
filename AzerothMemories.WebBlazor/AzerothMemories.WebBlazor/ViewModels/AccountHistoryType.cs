namespace AzerothMemories.WebBlazor.ViewModels;

public enum AccountHistoryType
{
    None,
    AccountCreated,
    UsernameChanged,
    MemoryRestored,
    MemoryDeleted,
    CharacterUpdated,

    StartedFollowing,
    StoppedFollowing,

    FollowingRequestSent,
    FollowingRequestReceived,
    FollowingRequestAccepted1,
    FollowingRequestAccepted2,
    FollowerRemoved,

    Commented1,
    Commented2,
    ReactedToPost1,
    ReactedToPost2,
    ReactedToComment1,
    ReactedToComment2,

    TaggedPost,
    TaggedComment,

    MemoryRestoredExternal1,
    MemoryRestoredExternal2,

    PostReported,
    PostReportedComment,
    PostReportedTags
}