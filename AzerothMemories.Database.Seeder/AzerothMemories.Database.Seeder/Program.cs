using AzerothMemories.WebServer.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

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
//services.AddSingleton<DatabaseProvider>();
services.AddSingleton<MoaResourceCache>();
services.AddSingleton<MoaResourceWriter>();
services.AddSingleton<MoaImageUploader>();
services.AddSingleton<HttpClientProvider>();

services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.EnableSensitiveDataLogging();
    optionsBuilder.UseNpgsql(config.DatabaseConnectionString, o => o.UseNodaTime());
});

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

await serviceProvider.GetRequiredService<MoaResourceWriter>().Initialize();

foreach (var func in seeders)
{
    await func(serviceProvider).Execute().ConfigureAwait(false);
}

await serviceProvider.GetRequiredService<MoaResourceWriter>().Save();
await serviceProvider.GetRequiredService<MoaImageUploader>().Upload();