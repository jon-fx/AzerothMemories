namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TrySetPostVisibility : ISessionCommand<byte?>
{
    public Post_TrySetPostVisibility(Session session, int postId, byte newVisibility)
    {
        Session = session;
        PostId = postId;
        NewVisibility = newVisibility;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public byte NewVisibility { get; init; }
}