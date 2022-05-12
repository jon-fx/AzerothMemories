namespace AzerothMemories.WebServer.Services.Commands;

public sealed record Guild_InvalidateGuildRecord(int GuildId, HashSet<int> CharacterIds);