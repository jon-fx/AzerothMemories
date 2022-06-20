namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    private const string BuildConst = "9.2.5.44232";

    public WowToolsInternal Main { get; } = new(BuildConst, true);
}