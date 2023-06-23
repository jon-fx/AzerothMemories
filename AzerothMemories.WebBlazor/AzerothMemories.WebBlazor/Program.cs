using AzerothMemories.WebBlazor;
using Stl.Fusion.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

ProgramEx.Initialize(builder.Services);

var fusion = builder.Services.AddFusion();
fusion.Rpc.AddWebSocketClient(builder.HostEnvironment.BaseAddress);
fusion.AddAuthClient();
fusion.AddRpcPeerConnectionMonitor();
fusion.AddBlazor().AddAuthentication().AddPresenceReporter();

//builder.Services.AddSingleton<RpcPeerFactory>(_ => static (hub, peerRef) => peerRef.IsServer ? throw new NotSupportedException() : new RpcClientPeer(hub, peerRef) { CallLogLevel = LogLevel.Debug });

fusion.AddClient<IAdminServices>();
fusion.AddClient<IAccountServices>();
fusion.AddClient<IFollowingServices>();
fusion.AddClient<ICharacterServices>();
fusion.AddClient<IGuildServices>();
fusion.AddClient<ITagServices>();
fusion.AddClient<IPostServices>();
fusion.AddClient<ISearchServices>();

var app = builder.Build();
app.Services.GetRequiredService<ComputeServices>().Initialize();

await app.RunAsync();