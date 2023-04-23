using AzerothMemories.Database.Seeder.Import;
using AzerothMemories.WebServer.Common;
using AzerothMemories.WebServer.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.BufferHeight = short.MaxValue - 1;

var config = new CommonConfig();
var services = new ServiceCollection();
services.AddSingleton(config);
services.AddLogging(configure => configure.AddConsole());

services.AddSingleton<MoaDatabaseWriter>();
services.AddSingleton<MoaImageUploader>();

services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.EnableSensitiveDataLogging();
    optionsBuilder.UseNpgsql(config.DatabaseConnectionString, o => o.UseNodaTime());
});

var serviceProvider = services.BuildServiceProvider(true);

await serviceProvider.GetRequiredService<MoaDatabaseWriter>().Initialize();
await serviceProvider.GetRequiredService<MoaDatabaseWriter>().Save();
await serviceProvider.GetRequiredService<MoaImageUploader>().Upload();

Console.WriteLine("*** DONE ***");
Console.ReadLine();