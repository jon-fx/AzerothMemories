namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AdminCountersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public long TimeStamp;

    [JsonInclude, DataMember, MemoryPackInclude] public int SessionCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int OperationCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int AcountCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int CharacterCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int GuildCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int PostCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int CommentCount;
    [JsonInclude, DataMember, MemoryPackInclude] public int UploadCount;
}