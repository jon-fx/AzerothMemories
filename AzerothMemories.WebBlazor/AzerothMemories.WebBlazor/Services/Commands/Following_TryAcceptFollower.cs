namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Following_TryAcceptFollower(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>;