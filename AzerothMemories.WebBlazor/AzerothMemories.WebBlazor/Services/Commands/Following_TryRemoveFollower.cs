namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryRemoveFollower(Session Session, long OtherAccountId) : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryRemoveFollower() : this(Session.Null, 0)
    {
    }
}