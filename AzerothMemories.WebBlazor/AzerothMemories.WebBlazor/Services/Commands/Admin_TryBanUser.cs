namespace AzerothMemories.WebBlazor.Services.Commands;

public record Admin_TryBanUser(Session Session, int AccountId, int Duration) : ISessionCommand<bool>
{
    public Admin_TryBanUser() : this(Session.Null, 0, 0)
    {
    }
}