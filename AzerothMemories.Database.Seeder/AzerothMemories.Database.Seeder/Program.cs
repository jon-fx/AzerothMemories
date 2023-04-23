using AzerothMemories.WebServer.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

Console.BufferHeight = short.MaxValue - 1;

var config = new CommonConfig();
var services = new ServiceCollection();
services.AddSingleton(config);
services.AddLogging(configure => configure.AddConsole());
services.AddHttpClient("Default", x =>
{
    x.DefaultRequestHeaders.Accept.Clear();
    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

services.AddSingleton<WowTools>();
services.AddSingleton<MoaResourceWriter>();
services.AddSingleton<HttpClientProvider>();

var seeders = new List<Func<ServiceProvider, AbstractBase>>();

void AddSeeder<TSeeder>() where TSeeder : AbstractBase
{
    services.AddSingleton<TSeeder>();
    seeders.Add(s => s.GetRequiredService<TSeeder>());
}

AddSeeder<CommonDataSeeder>();
AddSeeder<RealmDataSeeder>();
AddSeeder<PlayerDataSeeder>();
AddSeeder<AchievementDataSeeder>();
AddSeeder<ZoneDataSeeder>();
AddSeeder<CreatureDataSeeder>();
AddSeeder<QuestDataSeeder>();
AddSeeder<ItemSetDataSeeder>();
AddSeeder<TitleDataSeeder>();
AddSeeder<SpellDataSeeder>();
AddSeeder<MountDataSeeder>();
AddSeeder<ItemDataSeeder>();
AddSeeder<PetDataSeeder>();
AddSeeder<ToyDataSeeder>();

var serviceProvider = services.BuildServiceProvider(true);

await serviceProvider.GetRequiredService<MoaResourceWriter>().Initialize();

foreach (var func in seeders)
{
    await func(serviceProvider).Execute().ConfigureAwait(false);
}

await serviceProvider.GetRequiredService<MoaResourceWriter>().Save();

Console.WriteLine("*** DONE ***");
Console.ReadLine();