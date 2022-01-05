namespace AzerothMemories.WebBlazor.Services;

public sealed class AddMemoryTransferData
{
    [JsonInclude] public long TimeStamp { get; init; }
    [JsonInclude] public string AvatarTag { get; init; }
    [JsonInclude] public bool IsPrivate { get; init; }
    [JsonInclude] public string Comment { get; init; }
    [JsonInclude] public HashSet<string> SystemTags { get; init; }
    [JsonInclude] public List<AddMemoryUploadResult> UploadResults { get; init; }

    public AddMemoryTransferData()
    {
    }

    public AddMemoryTransferData(long timeStamp, string avatarTag, bool privatePost, string comment, HashSet<string> systemTags, List<AddMemoryUploadResult> uploadResults)
    {
        TimeStamp = timeStamp;
        //Avatar = avatar;
        AvatarTag = avatarTag;
        IsPrivate = privatePost;
        Comment = comment;
        SystemTags = systemTags;
        UploadResults = uploadResults;
    }
}