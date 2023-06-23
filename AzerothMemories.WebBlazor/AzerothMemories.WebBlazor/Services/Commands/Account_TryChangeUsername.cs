namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryChangeUsername : ISessionCommand<bool>
{
    public Account_TryChangeUsername(Session session, int accountId, string newUsername)
    {
        Session = session;
        AccountId = accountId;
        NewUsername = newUsername;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int AccountId { get; init; }

    [DataMember, MemoryPackInclude] public string NewUsername { get; init; }
}