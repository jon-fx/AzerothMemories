using ActualLab.Fusion.Blazor.Authentication;
using ActualLab.Fusion.EntityFramework.Npgsql;
using ActualLab.Fusion.Server.Authentication;
using ActualLab.Fusion.Server.Endpoints;
using AzerothMemories.WebBlazor;
using System.Net.Http.Headers;
using System.Text;

namespace AzerothMemories.WebServer;

public abstract class ProgramHelper
{
    private readonly CommonConfig _config;
    private readonly IServiceCollection _services;

    private FusionBuilder _fusion;
    private FusionWebServerBuilder _fusionServer;

    protected ProgramHelper(CommonConfig config, IServiceCollection services)
    {
        _config = config;
        _services = services;
    }

    public CommonConfig Config => _config;

    public IServiceCollection Services => _services;

    public FusionBuilder Fusion => _fusion;

    public FusionWebServerBuilder FusionWebServer => _fusionServer;

    public void Initialize()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ProgramEx.Initialize(_services);

#if DEBUG
        _services.AddDbContextFactory<AppDbContextBase>(ConfigureDbContextFactory);
#endif

        _services.AddTransientDbContextFactory<AppDbContext>(ConfigureDbContextFactory);

        _services.AddDbContextServices<AppDbContext>(dbContext =>
        {
            dbContext.AddOperations(operations =>
            {
                operations.AddNpgsqlOperationLogWatcher();
            });
        });

        _fusion = _services.AddFusion(RpcServiceMode.Server, true);
        _fusionServer = _fusion.AddWebServer(false);

        _fusion.AddDbAuthService<AppDbContext, string>();

        _fusionServer.ConfigureAuthEndpoint(_ => new AuthEndpoints.Options
        {
            SignInPropertiesBuilder = (_, properties) =>
            {
                properties.IsPersistent = true;
            }
        });

        _fusionServer.ConfigureServerAuthHelper(_ => new ServerAuthHelper.Options
        {
            NameClaimKeys = []
        });

        OnInitializeAuth();

        _fusion.AddOperationReprocessor();

        _services.AddSingleton(_config);
        _services.AddSingleton<CommonServices>();
        _services.AddSingleton<BlizzardUpdateHandler>();
        _services.AddSingleton<HttpClientProvider>();

        _fusion.AddComputeService<MediaServices>();
        _fusion.AddComputeService<BlizzardUpdateServices>();
        _fusion.AddComputeService<AccountServicesLocal>();

        _services.AddHttpClient("Blizzard", x =>
        {
            x.DefaultRequestHeaders.Accept.Clear();
            x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        _fusion.AddServer<IAdminServices, AdminServices>();
        _fusion.AddServer<IAccountServices, AccountServices>();
        _fusion.AddServer<IFollowingServices, FollowingServices>();
        _fusion.AddServer<ICharacterServices, CharacterServices>();
        _fusion.AddServer<IGuildServices, GuildServices>();
        _fusion.AddServer<ITagServices, TagServices>();
        _fusion.AddServer<IPostServices, PostServices>();
        _fusion.AddServer<ISearchServices, SearchServices>();

        _fusion.AddBlazor().AddAuthentication().AddPresenceReporter();
    }

    protected abstract void ConfigureDbContextFactory(DbContextOptionsBuilder optionsBuilder);

    protected abstract void OnInitializeAuth();

    public void Configure(IServiceProvider services)
    {
        services.GetRequiredService<CommonServices>().Initialize();
        services.GetRequiredService<ComputeServices>().Initialize();
        services.GetRequiredService<TimeProviderEx>().AlwaysUseUtc(true);
    }
}