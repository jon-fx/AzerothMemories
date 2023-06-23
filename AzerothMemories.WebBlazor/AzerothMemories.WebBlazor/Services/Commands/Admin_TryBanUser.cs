namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Admin_TryBanUser : ISessionCommand<bool>
{
    public Admin_TryBanUser(Session session, int accountId, long duration, string banReason)
    {
        Session = session;
        AccountId = accountId;
        Duration = duration;
        BanReason = banReason;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int AccountId { get; init; }

    [DataMember, MemoryPackInclude] public long Duration { get; init; }

    [DataMember, MemoryPackInclude] public string BanReason { get; init; }
}