namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Character_TryChangeCharacterAccountSync(Session Session, int CharacterId, bool NewValue) : ISessionCommand<bool>;