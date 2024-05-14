namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryChangeAvatarUpload : ISessionCommand<string?>
{
    public Account_TryChangeAvatarUpload(Session session, byte[] imageData)
    {
        Session = session;
        ImageData = imageData;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; } = null!;
    [DataMember, MemoryPackInclude] public byte[] ImageData { get; init; }
}