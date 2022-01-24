namespace AzerothMemories.WebBlazor.Services.Commands
{
    public record Account_TryChangeBattleTagVisibility(Session Session, bool NewValue) : ICommand<bool>
    {
        public Account_TryChangeBattleTagVisibility() : this(Session.Null, false)
        {
        }
    }
}