namespace AzerothMemories.WebServer.Services.Commands;

public record Guild_InvalidateGuildRecord(int GuildId, HashSet<int> CharacterIds)
{
    public Guild_InvalidateGuildRecord() : this(0, null)
    {
    }
}