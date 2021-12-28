using System.Diagnostics;

namespace AzerothMemories.WebServer.Database;

internal sealed class DatabaseProvider
{
    private readonly CommonConfig _commonConfig;
    private readonly ILogger<DatabaseProvider> _logger;
    private readonly LinqToDbConnectionOptions _databaseConfig;

    public DatabaseProvider(ILogger<DatabaseProvider> logger, CommonConfig commonConfig)
    {
        _logger = logger;
        _commonConfig = commonConfig;

        var builder = new LinqToDbConnectionOptionsBuilder();
        builder.UsePostgreSQL(_commonConfig.DatabaseConnectionString);
        builder.WithTraceLevel(TraceLevel.Verbose).WithTracing(x =>
        {
        });

        _databaseConfig = builder.Build();

        //using var database = GetDatabase();
        //database.DropTable<AccountRecord>();
        //database.CreateTable<AccountRecord>();

        //database.DropTable<CharacterRecord>();
        //database.CreateTable<CharacterRecord>();
    }

    public DatabaseConnection GetDatabase()
    {
        var database = new DatabaseConnection(_databaseConfig);
        return database;
    }
}