namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Following_TryRemoveFollower(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>;