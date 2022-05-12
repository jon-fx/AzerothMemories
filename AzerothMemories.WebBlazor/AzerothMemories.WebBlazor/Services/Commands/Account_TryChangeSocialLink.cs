namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Account_TryChangeSocialLink(Session Session, int AccountId, int LinkId, string NewValue) : ISessionCommand<string>;