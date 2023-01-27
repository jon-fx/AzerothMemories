namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    private const string BuildConst = "10.0.5.47849";

    public WowToolsInternal Main { get; } = new(BuildConst, true);
}