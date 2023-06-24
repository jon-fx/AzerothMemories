using AzerothMemories.WebBlazor;
using Stl.Fusion.Blazor.Authentication;
using Stl.Fusion.EntityFramework.Npgsql;
using Stl.Fusion.Server.Authentication;
using Stl.Fusion.Server.Controllers;
using System.Net.Http.Headers;
using System.Text;
using Stl.Fusion.Server.Endpoints;

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

        _services.AddDbContextFactory<AppDbContext>(ConfigureDbContextFactory);

        _services.AddTransient(_ => new DbOperationScope<AppDbContext>.Options
        {
            //DefaultIsolationLevel =  System.Data.IsolationLevel.Serializable,
        });

        _services.AddDbContextServices<AppDbContext>(dbContext =>
        {
            dbContext.AddOperations(operations =>
            {
                operations.ConfigureOperationLogReader(_ => new DbOperationLogReader<AppDbContext>.Options
                {
                    UnconditionalCheckPeriod = TimeSpan.FromSeconds(5),
                });

                operations.AddNpgsqlOperationLogChangeTracking();
            });
        });

        _fusion = _services.AddFusion(RpcServiceMode.Server, true);
        _fusionServer = _fusion.AddWebServer();

        _fusion.AddDbAuthService<AppDbContext, string>();

        _fusionServer.ConfigureAuthEndpoint(_ => new AuthEndpoints.Options
        {
            DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme,
            SignInPropertiesBuilder = (_, properties) =>
            {
                properties.IsPersistent = true;
            }
        });

        _fusionServer.ConfigureServerAuthHelper(_ => new ServerAuthHelper.Options
        {
            NameClaimKeys = Array.Empty<string>(),
        });

        OnInitializeAuth();

        _fusion.AddOperationReprocessor();

        _services.AddSingleton(_config);
        _services.AddSingleton<CommonServices>();
        _services.AddSingleton<BlizzardUpdateHandler>();
        _services.AddSingleton<HttpClientProvider>();
        
        _fusion.AddService<MediaServices>(RpcServiceMode.None);
        _fusion.AddService<BlizzardUpdateServices>(RpcServiceMode.None);

        _services.AddHttpClient("Blizzard", x =>
        {
            x.DefaultRequestHeaders.Accept.Clear();
            x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        void AddServiceWithSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService, IComputeService
        {
            _fusion.AddService<TService, TImplementation>();
            _services.AddSingleton(c => (TImplementation)c.GetRequiredService<TService>());
        }

        AddServiceWithSingleton<IAdminServices, AdminServices>();
        AddServiceWithSingleton<IAccountServices, AccountServices>();
        AddServiceWithSingleton<IFollowingServices, FollowingServices>();
        AddServiceWithSingleton<ICharacterServices, CharacterServices>();
        AddServiceWithSingleton<IGuildServices, GuildServices>();
        AddServiceWithSingleton<ITagServices, TagServices>();
        AddServiceWithSingleton<IPostServices, PostServices>();
        AddServiceWithSingleton<ISearchServices, SearchServices>();

        _fusion.AddBlazor().AddAuthentication().AddPresenceReporter();
    }

    protected abstract void ConfigureDbContextFactory(DbContextOptionsBuilder optionsBuilder);

    protected abstract void OnInitializeAuth();

    public void Configure(IServiceProvider services)
    {
        services.GetRequiredService<CommonServices>().Initialize();
        services.GetRequiredService<ComputeServices>().Initialize();
        services.GetRequiredService<TimeProvider>().AlwaysUseUtc(true);
    }
}