using Azure;

namespace AzerothMemories.WebServer.Services;

public record MediaUserResult : MediaResult
{
    public MediaUserResult()
    {
    }

    public MediaUserResult(Instant lastModified, ETag eTag, string mediaType, byte[]? mediaBytes, int postId, int postAccountId) : base(lastModified, eTag, mediaType, mediaBytes)
    {
        PostId = postId;
        PostAccountId = postAccountId;
    }

    public int PostId { get; }

    public int PostAccountId { get; }
}