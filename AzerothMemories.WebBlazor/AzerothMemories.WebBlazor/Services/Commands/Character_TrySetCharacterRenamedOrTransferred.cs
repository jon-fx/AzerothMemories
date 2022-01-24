namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_TrySetCharacterRenamedOrTransferred(Session Session, long OldCharacterId, long NewCharacterId) : ICommand<bool>
{
    public Character_TrySetCharacterRenamedOrTransferred() : this(Session.Null, 0, 0)
    {
    }
}