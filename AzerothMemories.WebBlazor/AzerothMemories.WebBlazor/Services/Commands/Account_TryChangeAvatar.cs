namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryChangeAvatar : ISessionCommand<string>
{
    public Account_TryChangeAvatar(Session session, int accountId, string newAvatar)
    {
        Session = session;
        AccountId = accountId;
        NewAvatar = newAvatar;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int AccountId { get; init; }

    [DataMember, MemoryPackInclude] public string NewAvatar { get; init; }
}