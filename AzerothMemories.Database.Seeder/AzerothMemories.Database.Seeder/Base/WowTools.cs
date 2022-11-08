namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    private const string BuildConst = "10.0.0.46547";

    public WowToolsInternal Main { get; } = new(BuildConst, true);
}