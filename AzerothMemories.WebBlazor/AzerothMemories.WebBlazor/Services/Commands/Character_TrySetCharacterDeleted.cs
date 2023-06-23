namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Character_TrySetCharacterDeleted : ISessionCommand<bool>
{
    public Character_TrySetCharacterDeleted(Session session, int characterId)
    {
        Session = session;
        CharacterId = characterId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int CharacterId { get; init; }
}