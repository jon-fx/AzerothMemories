namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryStopFollowing(Session Session, long OtherAccountId) : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryStopFollowing() : this(Session.Null, 0)
    {
    }
}