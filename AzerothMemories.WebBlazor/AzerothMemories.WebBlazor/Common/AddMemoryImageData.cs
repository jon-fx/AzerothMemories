namespace AzerothMemories.WebBlazor.Common;

public sealed class AddMemoryImageData
{
    public string FileName { get; init; }
    public long FileTimeStamp { get; init; }
    public byte[] FileContent { get; set; }
    public byte[] EditedFileContent { get; set; }
    public string ContentType { get; init; }
}