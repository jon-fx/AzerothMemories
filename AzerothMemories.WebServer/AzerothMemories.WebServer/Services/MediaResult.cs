using Azure;

namespace AzerothMemories.WebServer.Services;

public record MediaResult(Instant LastModified, ETag ETag, string MediaType, byte[] MediaBytes);