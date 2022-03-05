namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryAcceptFollower(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>
{
    public Following_TryAcceptFollower() : this(Session.Null, 0)
    {
    }
}