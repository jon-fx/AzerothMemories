namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_TrySetCharacterDeleted(Session Session, int CharacterId) : ISessionCommand<bool>
{
    public Character_TrySetCharacterDeleted() : this(Session.Null, 0)
    {
    }
}