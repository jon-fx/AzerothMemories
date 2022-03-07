using AzerothMemories.WebBlazor;
using Stl.Fusion.EntityFramework.Npgsql;
using Stl.Fusion.Server.Authentication;
using Stl.Fusion.Server.Controllers;
using System.Net.Http.Headers;
using System.Text;

var config = new CommonConfig();
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddAzureWebAppDiagnostics();
    //logging.SetMinimumLevel(LogLevel.Information);
    //if (Env.IsDevelopment()) {
    //logging.AddFilter("Microsoft", LogLevel.Warning);
    //logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
    //logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
    //}
});

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//EntityFrameworkPlusManager.IsCommunity = true;
ProgramEx.Initialize(builder.Services);

builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<BlizzardUpdateHostedService>();

builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    //optionsBuilder.EnableSensitiveDataLogging();
    optionsBuilder.UseNpgsql(config.DatabaseConnectionString, o => o.UseNodaTime());
});
builder.Services.AddTransient(c => new DbOperationScope<AppDbContext>(c)
{
    //IsolationLevel = System.Data.IsolationLevel.Serializable,
});
builder.Services.AddDbContextServices<AppDbContext>(dbContext =>
{
    dbContext.AddOperations((_, o) =>
    {
        o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(5);
    });

    dbContext.AddNpgsqlOperationLogChangeTracking();
    dbContext.AddAuthentication<string>();
});

builder.Services.AddSingleton(new Publisher.Options { Id = "p-67567567" });

var fusion = builder.Services.AddFusion();
var fusionServer = fusion.AddWebServer();

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

var fusionAuth = fusion.AddAuthentication().AddServer(_ => sessionMiddlewareSettings, _ => authHelperSettings, _ => signInControllerSettings);
var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

authenticationBuilder.AddPatreonAuth();
authenticationBuilder.AddBlizzardAuth(BlizzardRegion.Europe, config);
authenticationBuilder.AddBlizzardAuth(BlizzardRegion.Taiwan, config);
authenticationBuilder.AddBlizzardAuth(BlizzardRegion.Korea, config);
authenticationBuilder.AddBlizzardAuth(BlizzardRegion.UnitedStates, config);

authenticationBuilder.AddCookie(options =>
{
    options.LoginPath = "/signIn";
    options.LogoutPath = "/signOut";

    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnSigningIn = ctx =>
    {
        ctx.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(14);

        return Task.CompletedTask;
    };
});

builder.Services.AddServerSideBlazor(o => o.DetailedErrors = true);
fusionAuth.AddBlazor(_ => { });

builder.Services.AddSingleton(config);
builder.Services.AddSingleton<CommonServices>();
builder.Services.AddSingleton<BlizzardUpdateHandler>();
builder.Services.AddSingleton<WarcraftClientProvider>();

builder.Services.AddHttpClient("Blizzard", x =>
{
    x.DefaultRequestHeaders.Accept.Clear();
    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.UseRegisterAttributeScanner().RegisterFrom(typeof(CommonServices).Assembly);

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = TokenDelegatingHandler.HeaderName;
    options.Cookie.Name = "X-XSRF-TOKEN";
    //options.Cookie.SameSite = SameSiteMode.Strict;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();
app.Services.GetRequiredService<CommonServices>().Initialize();
app.Services.GetRequiredService<ComputeServices>().Initialize();
app.Services.GetRequiredService<TimeProvider>().AlwaysUseUtc(true);
app.UseSecurityHeaders(StartUpHelpers.GetHeaderPolicyCollection());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseStatusCodePages();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
}

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});
app.UseFusionSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapBlazorHub();
    endpoints.MapFusionWebSocketServer();
    endpoints.MapControllers();
    //endpoints.MapRazorPages();
    endpoints.MapFallbackToPage("/_Host");
});

app.Run();