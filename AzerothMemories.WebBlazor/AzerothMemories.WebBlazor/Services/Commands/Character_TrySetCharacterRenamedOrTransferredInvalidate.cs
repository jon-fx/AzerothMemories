namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Character_TrySetCharacterRenamedOrTransferredInvalidate(int OldAccountId, int OldCharacterId, int NewAccountId, int NewCharacterId, HashSet<int> PostIds);