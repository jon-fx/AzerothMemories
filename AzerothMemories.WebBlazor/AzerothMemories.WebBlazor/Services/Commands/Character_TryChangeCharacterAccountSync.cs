namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Character_TryChangeCharacterAccountSync : ISessionCommand<bool>
{
    public Character_TryChangeCharacterAccountSync(Session session, int characterId, bool newValue)
    {
        Session = session;
        CharacterId = characterId;
        NewValue = newValue;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int CharacterId { get; init; }

    [DataMember, MemoryPackInclude] public bool NewValue { get; init; }
}