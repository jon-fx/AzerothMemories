namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AdminCountersViewModel
{
    [JsonInclude] public long TimeStamp;

    [JsonInclude] public int SessionCount;
    [JsonInclude] public int OperationCount;

    [JsonInclude] public int AcountCount;
    [JsonInclude] public int CharacterCount;
    [JsonInclude] public int GuildCount;

    [JsonInclude] public int PostCount;
    [JsonInclude] public int CommentCount;
    [JsonInclude] public int UploadCount;
}