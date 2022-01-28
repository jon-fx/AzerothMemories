using Microsoft.EntityFrameworkCore;

namespace AzerothMemories.WebServer.Database;

public class AppDbContext : DbContextBase
{
    // Stl.Fusion.EntityFramework tables
    public DbSet<DbUser<string>> Users { get; protected set; } = null!;

    public DbSet<DbUserIdentity<string>> UserIdentities { get; protected set; } = null!;

    public DbSet<DbSessionInfo<string>> Sessions { get; protected set; } = null!;

    public DbSet<DbKeyValue> KeyValues { get; protected set; } = null!;

    public DbSet<DbOperation> Operations { get; protected set; } = null!;

    // AzerothMemories.WebServer.Database

    public DbSet<AccountRecord> Accounts { get; protected set; } = null!;

    public DbSet<AccountFollowingRecord> AccountFollowing { get; protected set; } = null!;

    public DbSet<AccountHistoryRecord> AccountHistory { get; protected set; } = null!;

    public DbSet<CharacterRecord> Characters { get; protected set; } = null!;

    public DbSet<CharacterAchievementRecord> CharacterAchievements { get; protected set; } = null!;

    public DbSet<GuildRecord> Guilds { get; protected set; } = null!;

    public DbSet<PostRecord> Posts { get; protected set; } = null!;

    public DbSet<PostTagRecord> PostTags { get; protected set; } = null!;

    public DbSet<PostReactionRecord> PostReactions { get; protected set; } = null!;

    public DbSet<PostCommentRecord> PostComments { get; protected set; } = null!;

    public DbSet<PostCommentReactionRecord> PostCommentReactions { get; protected set; } = null!;

    public DbSet<PostReportRecord> PostReports { get; protected set; } = null!;

    public DbSet<PostTagReportRecord> PostTagReports { get; protected set; } = null!;

    public DbSet<PostCommentReportRecord> PostCommentReports { get; protected set; } = null!;

    public DbSet<BlizzardDataRecord> BlizzardData { get; protected set; } = null!;

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}