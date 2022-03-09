namespace AzerothMemories.WebServer.Database;

public class AppDbContext : AppDbContextBase
{
    public DbSet<AccountRecord> Accounts { get; protected set; } = null!;

    public DbSet<AccountFollowingRecord> AccountFollowing { get; protected set; } = null!;

    public DbSet<AccountHistoryRecord> AccountHistory { get; protected set; } = null!;

    public DbSet<AccountUploadLog> UploadLogs { get; protected set; } = null!;

    public DbSet<CharacterRecord> Characters { get; protected set; } = null!;

    public DbSet<CharacterAchievementRecord> CharacterAchievements { get; protected set; } = null!;

    public DbSet<GuildRecord> Guilds { get; protected set; } = null!;

    public DbSet<GuildAchievementRecord> GuildAchievements { get; protected set; } = null!;

    public DbSet<PostRecord> Posts { get; protected set; } = null!;

    public DbSet<PostTagRecord> PostTags { get; protected set; } = null!;

    public DbSet<PostReactionRecord> PostReactions { get; protected set; } = null!;

    public DbSet<PostCommentRecord> PostComments { get; protected set; } = null!;

    public DbSet<PostCommentReactionRecord> PostCommentReactions { get; protected set; } = null!;

    public DbSet<PostReportRecord> PostReports { get; protected set; } = null!;

    public DbSet<PostTagReportRecord> PostTagReports { get; protected set; } = null!;

    public DbSet<PostCommentReportRecord> PostCommentReports { get; protected set; } = null!;

    public DbSet<BlizzardDataRecord> BlizzardData { get; protected set; } = null!;

    public DbSet<BlizzardUpdateRecord> BlizzardUpdates { get; protected set; } = null!;

    public DbSet<BlizzardUpdateChildRecord> BlizzardUpdateChildren { get; protected set; } = null!;

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountRecord>().Navigation(e => e.UpdateRecord).AutoInclude();
        modelBuilder.Entity<CharacterRecord>().Navigation(e => e.UpdateRecord).AutoInclude();
        modelBuilder.Entity<GuildRecord>().Navigation(e => e.UpdateRecord).AutoInclude();
    }
}