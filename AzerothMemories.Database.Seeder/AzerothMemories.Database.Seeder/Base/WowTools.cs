using AzerothMemories.WebServer.Common;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    public WowToolsInternal Main { get; } = new(CommonConfig.CurrentBuildConst, true);
}