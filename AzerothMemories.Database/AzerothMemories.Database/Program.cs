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
        .ScanIn(typeof(Migration0001).Assembly).For.Migrations());

var serviceProvider = services.BuildServiceProvider(true);

using var scope = serviceProvider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

if (ConfigHelpers.MigrateDown)
{
    ConfigHelpers.PushColors(ConsoleColor.White, ConsoleColor.Red);
    Console.WriteLine("DELETE DATA?");
    ConfigHelpers.PopColors();

    var result = Console.ReadLine();
    if (result == null || result.ToLower() != "y")
    {
        throw new NotImplementedException();
    }

    runner.MigrateDown(0);

    if (ConfigHelpers.ClearVersionDatabase && runner.Processor.TableExists("public", "VersionInfo"))
    {
        runner.Processor.Execute("DELETE FROM \"VersionInfo\"");
    }
}

runner.MigrateUp();