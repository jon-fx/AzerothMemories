namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryAcceptFollower(Session Session, long OtherAccountId) : ICommand<AccountFollowingStatus?>
{
    public Following_TryAcceptFollower() : this(Session.Null, 0)
    {
    }
}