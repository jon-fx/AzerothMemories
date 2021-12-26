namespace AzerothMemories.WebServer.Common;

public sealed class CommonConfig
{
    //public string DatabaseConnectionString { get; } = "***REMOVED***";

    //public string BlobStorageConnectionString { get; } = "***REMOVED***";

    //public string BlobStoragePath { get; } = "***REMOVED***";

    //public Duration ChangeUserNameDelay { get; } = Duration.FromSeconds(10);

    //public Duration UpdateAccountDelay { get; } = Duration.FromMinutes(1);

    //public Duration UpdateCharacterDelay { get; } = Duration.FromMinutes(10);

    //public Duration UpdateGuildDelay { get; } = Duration.FromMinutes(10);

    //public Duration CharacterSyncToggleDelay { get; } = Duration.FromSeconds(10);

    public readonly (string Id, string Secret)?[] BlizzardClientInfo =
    {
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***")
    };
}