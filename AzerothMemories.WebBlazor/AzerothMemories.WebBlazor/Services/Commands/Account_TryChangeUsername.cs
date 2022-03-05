namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeUsername(Session Session, int AccountId, string NewUsername) : ISessionCommand<bool>
{
    public Account_TryChangeUsername() : this(Session.Null, 0, null)
    {
    }
}