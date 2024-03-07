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

    public Instant LastModified { get; }

    public ETag ETag { get; }

    public string MediaType { get; }

    public byte[] MediaBytes { get; }
}