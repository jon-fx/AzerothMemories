namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryPostMemory : ISessionCommand<AddMemoryResult>
{
    [DataMember, MemoryPackInclude] public Session Session { get; init; }
    [DataMember, MemoryPackInclude] public long TimeStamp { get; init; }
    [DataMember, MemoryPackInclude] public string AvatarTag { get; init; }
    [DataMember, MemoryPackInclude] public bool IsPrivate { get; init; }
    [DataMember, MemoryPackInclude] public string Comment { get; init; }
    [DataMember, MemoryPackInclude] public HashSet<string> SystemTags { get; init; }
    [DataMember, MemoryPackInclude] public List<byte[]> ImageData { get; init; }
}