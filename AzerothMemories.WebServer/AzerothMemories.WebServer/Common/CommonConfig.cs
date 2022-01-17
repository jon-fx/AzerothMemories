namespace AzerothMemories.WebServer.Common;

public sealed class CommonConfig
{
    public string DatabaseConnectionString { get; } = "***REMOVED***";

    public string HangfireConnectionString { get; } = "***REMOVED***";

    //public string BlobStorageConnectionString { get; } = "***REMOVED***";

    //public string BlobStoragePath { get; } = "***REMOVED***";

    //public Duration ChangeUserNameDelay { get; } = Duration.FromSeconds(10);

    //public Duration UpdateAccountDelay { get; } = Duration.FromMinutes(1);

    //public Duration UpdateCharacterDelay { get; } = Duration.FromMinutes(10);

    //public Duration UpdateGuildDelay { get; } = Duration.FromMinutes(10);

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

    public const int PostsPerPage = 10;
    public const int CommentsPerPage = 50;
    public const int HistoryItemsPerPage = 50;

    public static readonly int TotalTagsMin;
    public static readonly int TotalTagsMax;
    public static readonly (int Min, int Max)[] TagCountsPerPost;

    static CommonConfig()
    {
        TagCountsPerPost = new (int Min, int Max)[(int)PostTagType.CountExcludingHashTag];
        TagCountsPerPost[(int)PostTagType.None] = (0, 0);

        TagCountsPerPost[(int)PostTagType.Type] = (1, 1);
        TagCountsPerPost[(int)PostTagType.Main] = (0, 10);

        TagCountsPerPost[(int)PostTagType.Region] = (1, 1);
        TagCountsPerPost[(int)PostTagType.Realm] = (0, 5);

        TagCountsPerPost[(int)PostTagType.Account] = (0, 50);
        TagCountsPerPost[(int)PostTagType.Character] = (0, 50);
        TagCountsPerPost[(int)PostTagType.Guild] = (0, 10);

        TagCountsPerPost[(int)PostTagType.Achievement] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Item] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Mount] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Pet] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Zone] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Npc] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Spell] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Object] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Quest] = (0, 10);
        TagCountsPerPost[(int)PostTagType.ItemSet] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Toy] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Title] = (0, 10);

        TagCountsPerPost[(int)PostTagType.CharacterRace] = (0, 10);
        TagCountsPerPost[(int)PostTagType.CharacterClass] = (0, 10);
        TagCountsPerPost[(int)PostTagType.CharacterClassSpecialization] = (0, 10);

        foreach (var tuple in TagCountsPerPost)
        {
            TotalTagsMin += tuple.Min;
            TotalTagsMax += tuple.Max;
        }
    }
}