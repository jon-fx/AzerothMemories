namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Account_TryChangeBattleTagVisibility(Session Session, bool NewValue) : ISessionCommand<bool>;