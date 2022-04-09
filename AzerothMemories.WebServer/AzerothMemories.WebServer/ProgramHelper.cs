using System.Net.Http.Headers;
using System.Text;
using AzerothMemories.WebBlazor;
using Stl.Fusion.EntityFramework.Npgsql;
using Stl.Fusion.Operations.Reprocessing;
using Stl.Fusion.Server.Authentication;
using Stl.Fusion.Server.Controllers;

namespace AzerothMemories.WebServer;

public abstract class ProgramHelper
{
    private readonly CommonConfig _config;
    private readonly IServiceCollection _services;

    private FusionBuilder _fusion;
    private FusionWebServerBuilder _fusionServer;
    private FusionAuthenticationBuilder _fusionAuth;

    protected ProgramHelper(CommonConfig config, IServiceCollection services)
    {
        _config = config;
        _services = services;
    }

    public CommonConfig Config => _config;

    public IServiceCollection Services => _services;

    public FusionBuilder Fusion => _fusion;

    public FusionWebServerBuilder FusionWebServer => _fusionServer;

    public FusionAuthenticationBuilder FusionAuthentication => _fusionAuth;

    public void Initialize()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ProgramEx.Initialize(_services);

#if DEBUG
        _services.AddDbContextFactory<AppDbContextBase>(ConfigureDbContextFactory);
#endif

        _services.AddDbContextFactory<AppDbContext>(ConfigureDbContextFactory);

        _services.AddTransient(c => new DbOperationScope<AppDbContext>(c)
        {
            //IsolationLevel = System.Data.IsolationLevel.Serializable,
        });
        _services.AddDbContextServices<AppDbContext>(dbContext =>
        {
            dbContext.AddOperations((_, o) =>
            {
                o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(5);
            });

            dbContext.AddNpgsqlOperationLogChangeTracking();
            dbContext.AddAuthentication<string>();
        });

        var generator = new Stl.Generators.RandomSymbolGenerator("p-", 12, "0123456789");
        _services.AddSingleton(new Publisher.Options { Id = generator.Next() });

        _fusion = _services.AddFusion();
        _fusionServer = _fusion.AddWebServer();

        _services.AddSingleton<ITransientFailureDetector>(new CustomTransientFailureDetector());
        _fusion.AddOperationReprocessor();

        var signInControllerSettings = SignInController.DefaultSettings with
        {
            DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme,
            SignInPropertiesBuilder = (_, properties) =>
            {
                properties.IsPersistent = true;
            }
        };

        var authHelperSettings = ServerAuthHelper.DefaultSettings with
        {
            NameClaimKeys = Array.Empty<string>(),
        };

        var sessionMiddlewareSettings = SessionMiddleware.DefaultSettings with
        {
        };

        var sessionFactory = new SessionFactory(new Stl.Generators.RandomStringGenerator(32));
        _services.AddSingleton<ISessionFactory>(sessionFactory);
        _fusionAuth = _fusion.AddAuthentication().AddServer(_ => sessionMiddlewareSettings, _ => authHelperSettings, _ => signInControllerSettings);

        OnInitializeAuth();

        _services.AddSingleton(_config);
        _services.AddSingleton<CommonServices>();
        _services.AddSingleton<BlizzardUpdateHandler>();
        _services.AddSingleton<HttpClientProvider>();

        _services.AddHttpClient("Blizzard", x =>
        {
            x.DefaultRequestHeaders.Accept.Clear();
            x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        _services.UseRegisterAttributeScanner().RegisterFrom(typeof(CommonServices).Assembly);
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