﻿namespace AzerothMemories.WebServer.Database;

public class AppDbContext : DbContextBase
{
    // Stl.Fusion.EntityFramework tables
    public DbSet<DbUser<string>> Users { get; protected set; } = null!;

    public DbSet<DbUserIdentity<string>> UserIdentities { get; protected set; } = null!;

    public DbSet<DbSessionInfo<string>> Sessions { get; protected set; } = null!;

    public DbSet<DbKeyValue> KeyValues { get; protected set; } = null!;

    public DbSet<DbOperation> Operations { get; protected set; } = null!;

    // Tables

    public DbSet<AccountRecord> Accounts { get; protected set; } = null!;

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}