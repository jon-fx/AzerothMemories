namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeAvatar(Session Session, string NewAvatar) : ISessionCommand<string>
{
    public Account_TryChangeAvatar() : this(Session.Null, null)
    {
    }
}