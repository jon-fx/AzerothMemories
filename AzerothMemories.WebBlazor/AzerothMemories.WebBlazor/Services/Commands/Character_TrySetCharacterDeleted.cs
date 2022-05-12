namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Character_TrySetCharacterDeleted(Session Session, int CharacterId) : ISessionCommand<bool>;