namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Following_TryStopFollowing : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryStopFollowing(Session session, int otherAccountId)
    {
        Session = session;
        OtherAccountId = otherAccountId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int OtherAccountId { get; init; }
}