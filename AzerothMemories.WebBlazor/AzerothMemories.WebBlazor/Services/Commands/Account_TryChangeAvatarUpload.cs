namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryChangeAvatarUpload : ISessionCommand<string>
{
    [DataMember, MemoryPackInclude] public Session Session { get; init; }
    [DataMember, MemoryPackInclude] public byte[] ImageData { get; init; }
}