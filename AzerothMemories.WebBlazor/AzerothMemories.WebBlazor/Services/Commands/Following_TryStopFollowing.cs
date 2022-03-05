namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryStopFollowing(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryStopFollowing() : this(Session.Null, 0)
    {
    }
}