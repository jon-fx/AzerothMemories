namespace AzerothMemories.WebServer.Common;

public sealed class CommonConfig
{
    public string DatabaseConnectionString { get; } = "***REMOVED***";

    public string HangfireConnectionString { get; } = "***REMOVED***";

    public string BlobStorageConnectionString { get; } = "***REMOVED***";

    public string BlobStoragePath { get; } = "***REMOVED***";

    //public Duration ChangeUserNameDelay { get; } = Duration.FromSeconds(10);

    public Duration UpdateAccountDelay { get; } = Duration.FromHours(1);

    public Duration UpdateCharacterHighDelay { get; } = Duration.FromHours(6);

    public Duration UpdateCharacterMedDelay { get; } = Duration.FromHours(12);

    public Duration UpdateCharacterLowDelay { get; } = Duration.FromDays(1);

    public Duration UpdateGuildDelay { get; } = Duration.FromDays(1);

    //public Duration CharacterSyncToggleDelay { get; } = Duration.FromSeconds(10);

    public readonly (string Id, string Secret)?[] BlizzardClientInfo =
    {
        null,
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***")
    };

    public void Initialize()
    {
        //DatabaseConnectionString = "***REMOVED***";
        //HangfireConnectionString = "***REMOVED***";
    }

    public const int PostsPerPage = 10;
    public const int CommentsPerPage = 50;
    public const int HistoryItemsPerPage = 50;
}