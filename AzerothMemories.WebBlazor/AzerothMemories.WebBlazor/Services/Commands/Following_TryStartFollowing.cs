namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_TryStartFollowing(Session Session, long OtherAccountId) : ICommand<AccountFollowingStatus?>
{
    public Following_TryStartFollowing() : this(Session.Null, 0)
    {
    }
}