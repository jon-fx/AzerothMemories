namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AdminCountersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public long TimeStamp { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int SessionCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int OperationCount { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int AccountCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int CharacterCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int GuildCount { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int PostCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int CommentCount { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public int UploadCount { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public AdminUpdateCountersViewModel UpdateCounters { get; init; }
}