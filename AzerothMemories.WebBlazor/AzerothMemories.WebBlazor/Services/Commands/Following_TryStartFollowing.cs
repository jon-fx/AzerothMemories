namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryStartFollowing(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryStartFollowing() : this(Session.Null, 0)
    {
    }
}