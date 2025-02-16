namespace AzerothMemories.WebServer.Services.Updates;

public sealed class UpdateHandlerInfo
{
    public UpdateHandlerInfo(BlizzardUpdateType updateType, string updateTypeString, CommonServices commonServices, ILogger<BlizzardUpdateServices> logger)
    {
        UpdateType = updateType;
        UpdateTypeString = updateTypeString;
        CommonServices = commonServices;
        Logger = logger;
    }

    public BlizzardUpdateType UpdateType { get; }
    public string UpdateTypeString { get; }
    public CommonServices CommonServices { get; }
    public ILogger<BlizzardUpdateServices> Logger { get; }
}