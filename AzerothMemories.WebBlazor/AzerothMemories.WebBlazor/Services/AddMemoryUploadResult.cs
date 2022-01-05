namespace AzerothMemories.WebBlazor.Services;

public sealed class AddMemoryUploadResult
{
    [JsonInclude] public string FileName { get; init; }
    [JsonInclude] public long FileTimeStamp { get; init; }
    [JsonInclude] public byte[] FileContent { get; init; }

    [JsonIgnore] public string ContentType { get; init; }
    [JsonIgnore] public string ContentBase64 { get; init; }
}