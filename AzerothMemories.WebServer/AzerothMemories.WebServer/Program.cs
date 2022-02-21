using AzerothMemories.WebBlazor;
using Hangfire;
using Hangfire.PostgreSql;
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

builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.EnableSensitiveDataLogging();
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
builder.Services.AddHangfire(options =>
{
    options.UsePostgreSqlStorage(config.HangfireConnectionString);
    Dapper.SqlMapper.AddTypeHandler(new DapperDateTimeTypeHandler());
});
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount;
    options.Queues = BlizzardUpdateHandler.AllQueues;
});

builder.Services.AddSingleton(new Publisher.Options { Id = "p-67567567" });

var fusion = builder.Services.AddFusion();
var fusionServer = fusion.AddWebServer();
var fusionAuth = fusion.AddAuthentication().AddServer(
    signInControllerSettingsFactory: _ => SignInController.DefaultSettings with
    {
        DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme,
        SignInPropertiesBuilder = (_, properties) =>
        {
            properties.IsPersistent = true;
        }
    },
    serverAuthHelperSettingsFactory: _ => ServerAuthHelper.DefaultSettings with
    {
        NameClaimKeys = Array.Empty<string>(),
    });

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
        ctx.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(28);
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
    //options.Cookie.Name = "__Host-X-XSRF-TOKEN";
    //options.Cookie.SameSite = SameSiteMode.Strict;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();
app.Services.GetRequiredService<CommonServices>().Initialize();
app.Services.GetRequiredService<ComputeServices>().Initialize();
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
    app.UseHangfireDashboard();
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

//var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
//await using var dbContext = dbContextFactory.CreateDbContext();

//await dbContext.Accounts.DeleteAsync();
//await dbContext.Characters.DeleteAsync();
//await dbContext.CharacterAchievements.DeleteAsync();
//await dbContext.Guilds.DeleteAsync();

//await dbContext.Accounts.UpdateAsync(x => new AccountRecord { UpdateJobEndTime = Instant.FromUnixTimeMilliseconds(0) });
//await dbContext.Characters.UpdateAsync(x => new CharacterRecord { UpdateJobEndTime = Instant.FromUnixTimeMilliseconds(0), BlizzardAchievementsLastModified = 0, BlizzardProfileLastModified = 0, BlizzardRendersLastModified = 0 });
//await dbContext.Guilds.UpdateAsync(x => new GuildRecord { UpdateJobEndTime = Instant.FromUnixTimeMilliseconds(0), BlizzardAchievementsLastModified = 0, BlizzardRosterLastModified = 0 });

app.Run();