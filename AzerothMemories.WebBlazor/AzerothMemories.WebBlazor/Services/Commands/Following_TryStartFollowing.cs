namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Following_TryStartFollowing : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryStartFollowing(Session session, int otherAccountId)
    {
        Session = session;
        OtherAccountId = otherAccountId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; } = null!;

    [DataMember, MemoryPackInclude] public int OtherAccountId { get; init; }
}