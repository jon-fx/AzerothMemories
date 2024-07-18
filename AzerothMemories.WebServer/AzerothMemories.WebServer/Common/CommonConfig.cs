namespace AzerothMemories.WebServer.Common;

public sealed class CommonConfig
{
    public CommonConfig()
    {
#if DEBUG
        DatabaseConnectionString = CommonConfigDoNotCommit.DatabaseConnectionString;
        BlobStorageConnectionString = CommonConfigDoNotCommit.BlobStorageConnectionString;

        UploadToBlobStorage = false;
#else
        DatabaseConnectionString = Environment.GetEnvironmentVariable("AZURE_POSTGRESQL_CONNECTIONSTRING").ThrowIfNull();
        BlobStorageConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTIONSTRING").ThrowIfNull();

        UploadToBlobStorage = true;
#endif
    }

    public string? DatabaseConnectionString { get; init; }

    public string BlobStorageConnectionString { get; init; }

    public Duration UpdateAccountDelay { get; } = Duration.FromHours(1);

    public Duration UsernameChangeDelay { get; set; } = Duration.FromDays(7);

    public bool UploadToBlobStorage { get; set; }

    public int UploadsInTheLastXCount { get; set; } = 30;

    public Duration UploadsInTheLastXDuration { get; set; } = Duration.FromMinutes(10);

    public int MaxUploadsWithTheSameHash { get; set; } = 10;

    public readonly (string Id, string Secret)?[] BlizzardClientInfo = CommonConfigDoNotCommit.BlizzardClientInfo;

    public string? PatreonClientId { get; set; } = CommonConfigDoNotCommit.PatreonClientId;

    public string? PatreonClientSecret { get; set; } = CommonConfigDoNotCommit.PatreonClientSecret;

    public string? PatreonCreatorsAccessToken { get; set; } = CommonConfigDoNotCommit.PatreonCreatorsAccessToken;
}