namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Following_TryRemoveFollower : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryRemoveFollower(Session session, int otherAccountId)
    {
        Session = session;
        OtherAccountId = otherAccountId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int OtherAccountId { get; init; }
}