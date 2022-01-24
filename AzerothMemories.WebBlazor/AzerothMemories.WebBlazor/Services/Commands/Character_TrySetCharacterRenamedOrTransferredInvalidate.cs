namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_TrySetCharacterRenamedOrTransferredInvalidate(long OldAccountId, long OldCharacterId, long NewAccountId, long NewCharacterId, HashSet<long> PostIds)
{
    public Character_TrySetCharacterRenamedOrTransferredInvalidate() : this(0, 0, 0, 0, null)
    {
    }
}