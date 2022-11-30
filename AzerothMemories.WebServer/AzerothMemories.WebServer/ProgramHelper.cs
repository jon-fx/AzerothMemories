using AzerothMemories.WebBlazor;
using Stl.Fusion.EntityFramework.Npgsql;
using Stl.Fusion.Server.Authentication;
using Stl.Fusion.Server.Controllers;
using Stl.Generators;
using System.Net.Http.Headers;
using System.Text;

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

            dbContext.AddAuthentication<string>();
        });

        _fusion = _services.AddFusion();
        _fusionServer = _fusion.AddWebServer();

        _services.AddSingleton(new PublisherOptions { Id = $"p-{RandomStringGenerator.Default.Next(8)}" });
        //_services.AddSingleton(new WebSocketServer.Options
        //{
        //    ConfigureWebSocket = () => new WebSocketAcceptContext
        //    {
        //        DangerousEnableCompression = false
        //    }
        //});

        _fusion.AddOperationReprocessor();
        //_services.TryAddEnumerable(ServiceDescriptor.Singleton(TransientFailureDetector.New(e => e is DbUpdateConcurrencyException)));
        //_services.TryAddEnumerable(ServiceDescriptor.Singleton(TransientFailureDetector.New(e => e is PostgresException postgresException && postgresException.IsTransient)));

        var signInControllerSettings = new SignInController.Options
        {
            DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme,
            SignInPropertiesBuilder = (_, properties) =>
            {
                properties.IsPersistent = true;
            }
        };

        var authHelperSettings = new ServerAuthHelper.Options
        {
            NameClaimKeys = Array.Empty<string>(),
        };

        var sessionMiddlewareSettings = new SessionMiddleware.Options
        {
        };

        //_services.AddScoped<ServerAuthHelper, CustomServerAuthHelper>();

        //var sessionFactory = new SessionFactory(new Stl.Generators.RandomStringGenerator(32));
        //_services.AddSingleton<ISessionFactory>(sessionFactory);
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