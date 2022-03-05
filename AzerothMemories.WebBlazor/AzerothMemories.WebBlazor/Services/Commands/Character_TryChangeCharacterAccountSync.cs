namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_TryChangeCharacterAccountSync(Session Session, int CharacterId, bool NewValue) : ISessionCommand<bool>
{
    public Character_TryChangeCharacterAccountSync() : this(Session.Null, 0, false)
    {
    }
}