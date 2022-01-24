namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeUsername(Session Session, string NewUsername) : ICommand<bool>
{
    public Account_TryChangeUsername() : this(Session.Null, null)
    {
    }
}