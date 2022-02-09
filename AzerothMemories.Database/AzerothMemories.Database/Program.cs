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
runner.MigrateDown(0);
runner.MigrateUp();