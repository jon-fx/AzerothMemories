using Azure;

namespace AzerothMemories.WebServer.Services;

public record MediaResult
{
    public MediaResult()
    {
        IsDefault = true;
    }

    public MediaResult(Instant lastModified, ETag eTag, string mediaType, byte[] mediaBytes)
    {
        LastModified = lastModified;
        ETag = eTag;
        MediaType = mediaType;
        MediaBytes = mediaBytes;
    }

    public bool IsDefault { get; init; }

    public Instant LastModified { get; init; }

    public ETag ETag { get; init; }

    public string MediaType { get; init; }

    public byte[] MediaBytes { get; init; }
}