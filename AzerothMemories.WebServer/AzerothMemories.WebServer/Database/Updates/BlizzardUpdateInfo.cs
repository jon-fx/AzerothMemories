namespace AzerothMemories.WebServer.Database.Updates;

public sealed class BlizzardUpdateInfo
{
    public long Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public Guid JobId { get; init; }
    public UpdatePriority Priority { get; init; }
}