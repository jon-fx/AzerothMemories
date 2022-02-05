namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeIsPrivate(Session Session, bool NewValue) : ISessionCommand<bool>
{
    public Account_TryChangeIsPrivate() : this(Session.Null, false)
    {
    }
}