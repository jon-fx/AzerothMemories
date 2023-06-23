namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Character_TrySetCharacterRenamedOrTransferredInvalidate
{
    public Character_TrySetCharacterRenamedOrTransferredInvalidate(int oldAccountId, int oldCharacterId, int newAccountId, int newCharacterId, HashSet<int> postIds)
    {
        OldAccountId = oldAccountId;
        OldCharacterId = oldCharacterId;
        NewAccountId = newAccountId;
        NewCharacterId = newCharacterId;
        PostIds = postIds;
    }

    [DataMember, MemoryPackInclude] public int OldAccountId { get; init; }

    [DataMember, MemoryPackInclude] public int OldCharacterId { get; init; }

    [DataMember, MemoryPackInclude] public int NewAccountId { get; init; }

    [DataMember, MemoryPackInclude] public int NewCharacterId { get; init; }

    [DataMember, MemoryPackInclude] public HashSet<int> PostIds { get; init; }
}