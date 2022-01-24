namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeAvatar(Session Session, string NewAvatar) : ICommand<string>
{
    public Account_TryChangeAvatar() : this(Session.Null, null)
    {
    }
}