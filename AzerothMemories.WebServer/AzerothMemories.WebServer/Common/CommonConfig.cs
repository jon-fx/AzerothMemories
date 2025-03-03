namespace AzerothMemories.WebServer.Common;

public sealed partial class CommonConfig
{
    public string DatabaseConnectionString { get; init; }

    public string BlobStorageConnectionString { get; init; }

    public Duration UpdateAccountDelay { get; } = Duration.FromHours(1);

    public Duration UsernameChangeDelay { get; set; } = Duration.FromDays(7);

    public bool UploadToBlobStorage { get; set; }

    public int UploadsInTheLastXCount { get; set; } = 30;

    public Duration UploadsInTheLastXDuration { get; set; } = Duration.FromMinutes(10);

    public int MaxUploadsWithTheSameHash { get; set; } = 10;

    public readonly (string Id, string Secret)?[] BlizzardClientInfo;

    public string? PatreonClientId { get; set; }

    public string? PatreonClientSecret { get; set; }

    public string? PatreonCreatorsAccessToken { get; set; }

    public string? BlobStorageAccount { get; init; }

    public string? BlobStorageAccountKey { get; init; }
}