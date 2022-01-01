using AzerothMemories.WebServer.Blizzard;
using AzerothMemories.WebServer.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

var commonConfig = new CommonConfig();
var services = new ServiceCollection();
services.AddSingleton(commonConfig);
services.AddLogging(configure => configure.AddConsole());
services.AddHttpClient("Default", x =>
{
    x.DefaultRequestHeaders.Accept.Clear();
    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

services.AddSingleton<WowTools>();
services.AddSingleton<MoaResourceCache>();
services.AddSingleton<MoaResourceWriter>();
services.AddSingleton<WarcraftClientProvider>();

//CommonSetup.SetUpCommon(services, commonConfig);

var seeders = new List<Func<ServiceProvider, AbstractBase>>();

void AddSeeder<TSeeder>() where TSeeder : AbstractBase
{
    services.AddSingleton<TSeeder>();
    seeders.Add(s => s.GetRequiredService<TSeeder>());
}

AddSeeder<CommonDataSeeder>();
AddSeeder<RealmDataSeeder>();
AddSeeder<PlayerDataSeeder>();
//AddSeeder<InstanceSeeder>();
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

foreach (var func in seeders)
{
    await func(serviceProvider).Execute().ConfigureAwait(false);
}

serviceProvider.GetRequiredService<MoaResourceWriter>().Save();