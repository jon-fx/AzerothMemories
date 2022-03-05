namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_TrySetCharacterRenamedOrTransferredInvalidate(int OldAccountId, int OldCharacterId, int NewAccountId, int NewCharacterId, HashSet<int> PostIds)
{
    public Character_TrySetCharacterRenamedOrTransferredInvalidate() : this(0, 0, 0, 0, null)
    {
    }
}