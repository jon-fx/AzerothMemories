namespace AzerothMemories.WebServer.Database;

public class AppDbContextBase : DbContextBase
{
    public AppDbContextBase(DbContextOptions options) : base(options)
    {
    }

    public DbSet<DbUser<string>> Users { get; protected init; } = null!;

    public DbSet<DbUserIdentity<string>> UserIdentities { get; protected init; } = null!;

    public DbSet<DbSessionInfo<string>> Sessions { get; protected init; } = null!;

    public DbSet<DbKeyValue> KeyValues { get; protected init; } = null!;

    public DbSet<DbOperation> Operations { get; protected init; } = null!;

    public DbSet<DbOperationEvent> OperationEvents { get; protected set; } = null!;

    public DbSet<DbOperationTimer> OperationTimers { get; protected set; } = null!;
}