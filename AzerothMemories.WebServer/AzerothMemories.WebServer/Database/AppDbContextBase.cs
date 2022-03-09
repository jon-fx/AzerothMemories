namespace AzerothMemories.WebServer.Database;

public class AppDbContextBase : DbContextBase
{
    public AppDbContextBase(DbContextOptions options) : base(options)
    {
    }

    public DbSet<DbUser<string>> Users { get; protected set; } = null!;

    public DbSet<DbUserIdentity<string>> UserIdentities { get; protected set; } = null!;

    public DbSet<DbSessionInfo<string>> Sessions { get; protected set; } = null!;

    public DbSet<DbKeyValue> KeyValues { get; protected set; } = null!;

    public DbSet<DbOperation> Operations { get; protected set; } = null!;
}