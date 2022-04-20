namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    private const string BuildConst = "9.2.0.43206";

    public WowToolsInternal Main { get; } = new(BuildConst, true);
}