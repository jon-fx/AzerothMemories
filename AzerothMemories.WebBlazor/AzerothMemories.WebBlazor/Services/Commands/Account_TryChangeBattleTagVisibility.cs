namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeBattleTagVisibility(Session Session, bool NewValue) : ISessionCommand<bool>
{
    public Account_TryChangeBattleTagVisibility() : this(Session.Null, false)
    {
    }
}