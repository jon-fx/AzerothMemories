namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryRestoreMemory : ISessionCommand<bool>
{
    public Post_TryRestoreMemory(Session session, int postId, int newCharacterId)
    {
        Session = session;
        PostId = postId;
        NewCharacterId = newCharacterId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public int NewCharacterId { get; init; }
}