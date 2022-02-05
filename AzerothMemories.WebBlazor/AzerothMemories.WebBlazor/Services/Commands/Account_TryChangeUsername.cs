namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeUsername(Session Session, string NewUsername) : ISessionCommand<bool>
{
    public Account_TryChangeUsername() : this(Session.Null, null)
    {
    }
}