namespace AzerothMemories.WebServer.Services.Commands;

public sealed record Character_TrySetCharacterRenamedOrTransferredInvalidate
{
    public Character_TrySetCharacterRenamedOrTransferredInvalidate(int oldAccountId, int oldCharacterId, int newAccountId, int newCharacterId, HashSet<int> postIds)
    {
        OldAccountId = oldAccountId;
        OldCharacterId = oldCharacterId;
        NewAccountId = newAccountId;
        NewCharacterId = newCharacterId;
        PostIds = postIds;
    }

    public int OldAccountId { get; init; }

    public int OldCharacterId { get; init; }

    public int NewAccountId { get; init; }

    public int NewCharacterId { get; init; }

    public HashSet<int>? PostIds { get; init; }
}