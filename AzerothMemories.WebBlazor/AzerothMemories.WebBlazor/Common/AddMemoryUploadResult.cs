﻿namespace AzerothMemories.WebBlazor.Common;

public sealed class AddMemoryUploadResult
{
    [JsonInclude] public string FileName { get; init; }
    [JsonInclude] public long FileTimeStamp { get; init; }
    [JsonInclude] public byte[] FileContent { get; init; }

    [JsonIgnore] public string ContentType { get; init; }
}