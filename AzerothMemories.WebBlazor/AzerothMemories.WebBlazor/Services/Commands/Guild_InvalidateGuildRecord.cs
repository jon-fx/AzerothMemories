namespace AzerothMemories.WebBlazor.Services.Commands;

public record Guild_InvalidateGuildRecord(long GuildId)
{
    public Guild_InvalidateGuildRecord() : this(0)
    {
    }
}