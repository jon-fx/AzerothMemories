namespace AzerothMemories.WebServer.Services.Commands;

public record Character_InvalidateCharacterRecord(int CharacterId, int AccountId)
{
    public Character_InvalidateCharacterRecord() : this(0, 0)
    {
    }
}