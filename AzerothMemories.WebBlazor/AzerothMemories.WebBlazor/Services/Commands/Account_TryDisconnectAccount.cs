namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryDisconnectAccount(Session Session, string Schemna, string Key) : ISessionCommand<bool>
{
    public Account_TryDisconnectAccount() : this(Session.Null, null, null)
    {
    }
}