namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    private const string BuildConst = "9.2.7.45338";

    public WowToolsInternal Main { get; } = new(BuildConst, true);
}