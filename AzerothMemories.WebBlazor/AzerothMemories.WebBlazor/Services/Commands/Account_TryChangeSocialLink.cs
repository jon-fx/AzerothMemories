namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryChangeSocialLink : ISessionCommand<string>
{
    public Account_TryChangeSocialLink(Session session, int accountId, int linkId, string newValue)
    {
        Session = session;
        AccountId = accountId;
        LinkId = linkId;
        NewValue = newValue;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int AccountId { get; init; }

    [DataMember, MemoryPackInclude] public int LinkId { get; init; }

    [DataMember, MemoryPackInclude] public string NewValue { get; init; }
}