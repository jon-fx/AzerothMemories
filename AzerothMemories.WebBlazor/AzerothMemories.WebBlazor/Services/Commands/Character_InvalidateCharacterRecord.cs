namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_InvalidateCharacterRecord(long CharacterId, long AccountId)
{
    public Character_InvalidateCharacterRecord() : this(0, 0)
    {
    }
}