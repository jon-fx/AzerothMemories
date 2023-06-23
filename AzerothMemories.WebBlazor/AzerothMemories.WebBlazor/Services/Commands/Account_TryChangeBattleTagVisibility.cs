namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryChangeBattleTagVisibility : ISessionCommand<bool>
{
    public Account_TryChangeBattleTagVisibility(Session session, bool newValue)
    {
        Session = session;
        NewValue = newValue;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public bool NewValue { get; init; }
}