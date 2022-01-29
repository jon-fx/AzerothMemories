namespace AzerothMemories.WebBlazor.Services.Commands;

public record Guild_InvalidateGuildRecord(long GuildId, HashSet<long> CharacterIds)
{
    public Guild_InvalidateGuildRecord() : this(0, null)
    {
    }
}