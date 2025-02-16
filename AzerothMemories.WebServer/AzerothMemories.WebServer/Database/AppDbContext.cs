namespace AzerothMemories.WebServer.Database;

public class AppDbContext : AppDbContextBase
{
    private static readonly Type[] _recordTypesWithRowVersion;

    static AppDbContext()
    {
        _recordTypesWithRowVersion = typeof(IDatabaseRecordWithVersion).Assembly.GetTypes()
            .Where(x => typeof(IDatabaseRecordWithVersion).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .ToArray();
    }

    public DbSet<AccountRecord> Accounts { get; protected init; } = null!;

    public DbSet<AuthTokenRecord> AuthTokens { get; protected init; } = null!;

    public DbSet<AccountFollowingRecord> AccountFollowing { get; protected init; } = null!;

    public DbSet<AccountHistoryRecord> AccountHistory { get; protected init; } = null!;

    public DbSet<AccountUploadLog> UploadLogs { get; protected init; } = null!;

    public DbSet<CharacterRecord> Characters { get; protected init; } = null!;

    public DbSet<CharacterAchievementRecord> CharacterAchievements { get; protected init; } = null!;

    public DbSet<CharacterFirstAchievementRecord> CharacterFirstAchievements { get; protected init; } = null!;

    public DbSet<CharacterAchievementStatisticRecord> CharacterAchievementStatistics { get; protected init; } = null!;

    public DbSet<CharacterMountRecord> CharacterMounts { get; protected init; } = null!;

    public DbSet<GuildRecord> Guilds { get; protected init; } = null!;

    public DbSet<GuildAchievementRecord> GuildAchievements { get; protected init; } = null!;

    public DbSet<PostRecord> Posts { get; protected init; } = null!;

    public DbSet<PostTagRecord> PostTags { get; protected init; } = null!;

    public DbSet<PostReactionRecord> PostReactions { get; protected init; } = null!;

    public DbSet<PostCommentRecord> PostComments { get; protected init; } = null!;

    public DbSet<PostCommentReactionRecord> PostCommentReactions { get; protected init; } = null!;

    public DbSet<PostReportRecord> PostReports { get; protected init; } = null!;

    public DbSet<PostTagReportRecord> PostTagReports { get; protected init; } = null!;

    public DbSet<PostCommentReportRecord> PostCommentReports { get; protected init; } = null!;

    public DbSet<BlizzardDataRecord> BlizzardData { get; protected init; } = null!;

    public DbSet<BlizzardUpdateRecord> BlizzardUpdates { get; protected init; } = null!;

    public DbSet<BlizzardUpdateChildRecord> BlizzardUpdateChildren { get; protected init; } = null!;

    public DbSet<BlizzardRealmRecord> BlizzardRealms { get; protected init; } = null!;

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountRecord>().Navigation(e => e.AuthTokens).AutoInclude();
        modelBuilder.Entity<AuthTokenRecord>().Navigation(e => e.Account).AutoInclude();

        modelBuilder.Entity<AccountRecord>().Navigation(e => e.UpdateRecord).AutoInclude();
        modelBuilder.Entity<CharacterRecord>().Navigation(e => e.UpdateRecord).AutoInclude();
        modelBuilder.Entity<GuildRecord>().Navigation(e => e.UpdateRecord).AutoInclude();

        //modelBuilder.Entity<BlizzardUpdateRecord>().Navigation(e => e.AuthToken).AutoInclude();
        //modelBuilder.Entity<BlizzardUpdateRecord>().Navigation(e => e.Character).AutoInclude();
        //modelBuilder.Entity<BlizzardUpdateRecord>().Navigation(e => e.Guild).AutoInclude();
        modelBuilder.Entity<BlizzardUpdateRecord>().Navigation(e => e.Children).AutoInclude();

        foreach (var type in _recordTypesWithRowVersion)
        {
            modelBuilder.Entity(type).Property(nameof(IDatabaseRecordWithVersion.RowVersion))
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}