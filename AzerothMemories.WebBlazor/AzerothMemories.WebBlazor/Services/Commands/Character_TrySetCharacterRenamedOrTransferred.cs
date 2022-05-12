namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Character_TrySetCharacterRenamedOrTransferred(Session Session, int OldCharacterId, int NewCharacterId) : ISessionCommand<bool>;