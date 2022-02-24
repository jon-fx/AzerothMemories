namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeAvatar(Session Session, long AccountId, string NewAvatar) : ISessionCommand<string>
{
    public Account_TryChangeAvatar() : this(Session.Null, 0, null)
    {
    }
}