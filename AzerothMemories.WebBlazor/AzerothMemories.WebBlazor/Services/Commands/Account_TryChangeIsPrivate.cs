namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeIsPrivate(Session Session, bool NewValue) : ICommand<bool>
{
    public Account_TryChangeIsPrivate() : this(Session.Null, false)
    {
    }
}