namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Character_TrySetCharacterRenamedOrTransferred : ISessionCommand<bool>
{
    public Character_TrySetCharacterRenamedOrTransferred(Session session, int oldCharacterId, int newCharacterId)
    {
        Session = session;
        OldCharacterId = oldCharacterId;
        NewCharacterId = newCharacterId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int OldCharacterId { get; init; }

    [DataMember, MemoryPackInclude] public int NewCharacterId { get; init; }
}