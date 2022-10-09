using AzerothMemories.WebBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<HeadOutlet>("head::after");

ProgramEx.Initialize(builder.Services);

var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseUri = new Uri($"{baseUri}api/");

// Fusion services
var fusion = builder.Services.AddFusion();
var restEaseClientBuilder = fusion.AddRestEaseClient();
restEaseClientBuilder.ConfigureHttpClient((_, name, o) =>
{
    var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
    var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
    o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
});

restEaseClientBuilder.ConfigureWebSocketChannel(_ => new WebSocketChannelProvider.Options
{
    BaseUri = baseUri,
    //IsLoggingEnabled = true,
    //IsMessageLoggingEnabled = false
});

restEaseClientBuilder.AddReplicaService<IAdminServices>();
restEaseClientBuilder.AddReplicaService<IAccountServices>();
restEaseClientBuilder.AddReplicaService<IFollowingServices>();
restEaseClientBuilder.AddReplicaService<ICharacterServices>();
restEaseClientBuilder.AddReplicaService<IGuildServices>();
restEaseClientBuilder.AddReplicaService<ITagServices>();
restEaseClientBuilder.AddReplicaService<IPostServices>();
restEaseClientBuilder.AddReplicaService<ISearchServices>();

fusion.AddAuthentication().AddRestEaseClient().AddBlazor();

var app = builder.Build();
app.Services.GetRequiredService<ComputeServices>().Initialize();

await app.RunAsync();