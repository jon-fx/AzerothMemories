namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryRemoveFollower(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryRemoveFollower() : this(Session.Null, 0)
    {
    }
}