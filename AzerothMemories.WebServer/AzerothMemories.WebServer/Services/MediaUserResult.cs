using Azure;

namespace AzerothMemories.WebServer.Services;

public record MediaUserResult(Instant LastModified, ETag ETag, string MediaType, byte[] MediaBytes, int PostId, int PostAccountId) : MediaResult(LastModified, ETag,MediaType, MediaBytes);