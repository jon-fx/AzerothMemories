namespace AzerothMemories.WebServer.Common;

public sealed class CommonConfig
{
    public CommonConfig()
    {
        //DatabaseConnectionString = "***REMOVED***";
        //HangfireConnectionString = "***REMOVED***";

        DatabaseConnectionString = "***REMOVED***";
        HangfireConnectionString = "***REMOVED***";
    }

    public string DatabaseConnectionString { get; init; }

    public string HangfireConnectionString { get; init; }

    public string BlobStorageConnectionString { get; } = "***REMOVED***";

    public Duration UpdateAccountDelay { get; } = Duration.FromHours(1);

    public Duration UpdateCharacterHighDelay { get; } = Duration.FromHours(6);

    public Duration UpdateCharacterMedDelay { get; } = Duration.FromHours(12);

    public Duration UpdateCharacterLowDelay { get; } = Duration.FromDays(1);

    public Duration UpdateGuildDelay { get; } = Duration.FromDays(1);

    public bool UploadToBlobStorage { get; set; } = !true;

    public bool UpdateSkipCharactersOnLowPriority { get; set; } = !true;

    public int UploadsInTheLastXCount { get; set; } = 30;

    public Duration UploadsInTheLastXDuration { get; set; } = Duration.FromMinutes(10);

    public int MaxUploadsWithTheSameHash { get; set; } = 10;

    public readonly (string Id, string Secret)?[] BlizzardClientInfo =
    {
        null,
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***"),
        new("***REMOVED***", "***REMOVED***")
    };

    public const int PostsPerPage = 10;
    public const int CommentsPerPage = 50;
    public const int HistoryItemsPerPage = 50;
}