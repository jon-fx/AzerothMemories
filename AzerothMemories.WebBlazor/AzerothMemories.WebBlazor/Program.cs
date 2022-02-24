using AzerothMemories.WebBlazor;
using Microsoft.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<HeadOutlet>("head::after");

ProgramEx.Initialize(builder.Services);

var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseUri = new Uri($"{baseUri}api/");

// Fusion services
var fusion = builder.Services.AddFusion();
var fusionClient = fusion.AddRestEaseClient((_, o) =>
{
    o.BaseUri = baseUri;
    o.IsLoggingEnabled = true;
    o.IsMessageLoggingEnabled = false;
});
fusionClient.ConfigureHttpClientFactory((c, name, o) =>
{
    var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
    var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
    o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
});

builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
{
    options.HttpMessageHandlerBuilderActions.Add(options2 =>
    {
        options2.AdditionalHandlers.Add(options2.Services.GetRequiredService<TokenDelegatingHandler>());
    });
});

builder.Services.AddScoped<TokenDelegatingHandler>();

fusionClient.AddReplicaService<IAdminServices>();
fusionClient.AddReplicaService<IAccountServices>();
fusionClient.AddReplicaService<IFollowingServices>();
fusionClient.AddReplicaService<ICharacterServices>();
fusionClient.AddReplicaService<IGuildServices>();
fusionClient.AddReplicaService<ITagServices>();
fusionClient.AddReplicaService<IPostServices>();
fusionClient.AddReplicaService<ISearchServices>();
fusion.AddAuthentication().AddRestEaseClient().AddBlazor();

var app = builder.Build();
//app.Services.GetRequiredService<CommonServices>().Initialize();
app.Services.GetRequiredService<ComputeServices>().Initialize();

await app.RunAsync();