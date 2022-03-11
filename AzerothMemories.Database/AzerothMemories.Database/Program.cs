using AzerothMemories.Database;
using AzerothMemories.Database.Migrations;
using AzerothMemories.WebServer.Common;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

var config = new CommonConfig();
var services = new ServiceCollection();

services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(config.DatabaseConnectionString)
        .ScanIn(typeof(Migration0001_EntiyFramework).Assembly).For.Migrations());

if (config.DatabaseConnectionString.Contains("azure"))
{
    if (ConfigHelpers.SaftyCheck("DO YOU REALLY WANT TO MODIFY AN AZURE DATABASE?!"))
    {
    }
    else
    {
        throw new NotImplementedException();
    }
}

var serviceProvider = services.BuildServiceProvider(true);

using var scope = serviceProvider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

if (ConfigHelpers.SaftyCheck("DELETE ABSOLUTELY EVERYTHING?!"))
{
    runner.MigrateDown(Migration0001_EntiyFramework.MigrationId - 1);

    runner.Processor.Execute("DELETE FROM \"VersionInfo\"");
}
else if (ConfigHelpers.SaftyCheck("DELETE BLIZZARD DATA AND ACCOUNTS?!"))
{
    runner.MigrateDown(Migration0002_BlizzardData.MigrationId - 1);
}
else if (ConfigHelpers.SaftyCheck("DELETE ACCOUNT DATA?!"))
{
    runner.MigrateDown(Migration0003_AccountData.MigrationId - 1);
}

ConfigHelpers.WriteWithColors(ConsoleColor.Green, ConsoleColor.White, "Running Migration...");

runner.MigrateUp();