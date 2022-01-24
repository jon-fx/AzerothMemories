namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_TrySetCharacterDeleted(Session Session, long CharacterId) : ICommand<bool>
{
    public Character_TrySetCharacterDeleted() : this(Session.Null, 0)
    {
    }
}