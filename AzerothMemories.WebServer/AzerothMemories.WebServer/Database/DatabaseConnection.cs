namespace AzerothMemories.WebServer.Database;

public sealed class DatabaseConnection : DataConnection
{
    public DatabaseConnection(LinqToDbConnectionOptions databaseConfig) : base(databaseConfig)
    {
    }

    public ITable<AccountRecord> Accounts => GetTable<AccountRecord>();

    public ITable<AccountFollowingRecord> AccountFollowing => GetTable<AccountFollowingRecord>();

    public ITable<CharacterRecord> Characters => GetTable<CharacterRecord>();

    public ITable<CharacterAchievementRecord> CharacterAchievements => GetTable<CharacterAchievementRecord>();

    public ITable<GuildRecord> Guilds => GetTable<GuildRecord>();

    //public ITable<TagRecord> Tags => GetTable<TagRecord>();

    public ITable<PostRecord> Posts => GetTable<PostRecord>();

    public ITable<PostTagRecord> PostTags => GetTable<PostTagRecord>();

    public ITable<PostReactionRecord> PostReactions => GetTable<PostReactionRecord>();

    public ITable<PostCommentRecord> PostComments => GetTable<PostCommentRecord>();

    public ITable<PostCommentReactionRecord> PostCommentReactions => GetTable<PostCommentReactionRecord>();

    //public ITable<PostReportRecord> PostReports => GetTable<PostReportRecord>();

    //public ITable<PostTagReportRecord> PostTagReports => GetTable<PostTagReportRecord>();

    //public ITable<PostCommentReportRecord> PostCommentReports => GetTable<PostCommentReportRecord>();

    //public ITable<AccountHistoryRecord> AccountHistory => GetTable<AccountHistoryRecord>();

    public ITable<BlizzardDataRecord> BlizzardData => GetTable<BlizzardDataRecord>();

    public IUpdatable<TRecord> GetUpdateQuery<TRecord>(TRecord record, out bool changed) where TRecord : class, IDatabaseRecord
    {
        Exceptions.ThrowIf(record.Id == 0);

        changed = false;

        return GetTable<TRecord>().Where(x => x.Id == record.Id).AsUpdatable();
    }
}