using AzerothMemories.WebServer.Database.Migrations;
using FluentMigrator.Runner;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;

var config = new CommonConfig();
var builder = WebApplication.CreateBuilder(args);

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddMudServices();

builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appTempDir = FilePath.GetApplicationTempDirectory("", true);
var dbPath = appTempDir & "App.db";
builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(config.DatabaseConnectionString);

    //if (builder.Environment.IsDevelopment())
    //{
    //    optionsBuilder.EnableSensitiveDataLogging();
    //}
});
builder.Services.AddTransient(c => new DbOperationScope<AppDbContext>(c)
{
    IsolationLevel = IsolationLevel.Serializable,
});
builder.Services.AddDbContextServices<AppDbContext>(dbContext =>
{
    dbContext.AddOperations((_, o) =>
    {
        o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(builder.Environment.IsDevelopment() ? 60 : 5);
    });

    var operationLogChangeAlertPath = dbPath + "_changed";
    dbContext.AddFileBasedOperationLogChangeTracking(operationLogChangeAlertPath);

    dbContext.AddAuthentication<string>();
    //dbContext.AddAuthentication((_, options) =>
    //{
    //    options.MinUpdatePresencePeriod = TimeSpan.FromSeconds(55);
    //});

    //dbContext.AddKeyValueStore();
});

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(config.DatabaseConnectionString)
        .ScanIn(typeof(Migration0001).Assembly).For.Migrations());

builder.Services.AddHangfire(options =>
{
    options.UsePostgreSqlStorage(config.HangfireConnectionString);
});
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { BlizzardUpdateHandler.AccountQueue1, BlizzardUpdateHandler.CharacterQueue1, BlizzardUpdateHandler.CharacterQueue2, BlizzardUpdateHandler.GuildQueue1 };
});

builder.Services.AddSingleton(new Publisher.Options { Id = "p-67567567" });

var fusion = builder.Services.AddFusion();
var fusionServer = fusion.AddWebServer();
//var fusionClient = fusion.AddRestEaseClient();
var fusionAuth = fusion.AddAuthentication().AddServer(
    signInControllerOptionsBuilder: (_, options) =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });

var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

authenticationBuilder.AddOAuth(BlizzardRegion.Europe);
authenticationBuilder.AddOAuth(BlizzardRegion.Taiwan);
authenticationBuilder.AddOAuth(BlizzardRegion.Korea);
authenticationBuilder.AddOAuth(BlizzardRegion.UnitedStates);

authenticationBuilder.AddCookie(options =>
{
    options.LoginPath = "/signIn";
    options.LogoutPath = "/signOut";

    //if (builder.Environment.IsDevelopment())
    //{
    //    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    //}
});

builder.Services.AddServerSideBlazor(o => o.DetailedErrors = true);
fusionAuth.AddBlazor(o => { }); // Must follow services.AddServerSideBlazor()!

builder.Services.AddSingleton(config);
builder.Services.AddSingleton<CommonServices>();
builder.Services.AddSingleton<DatabaseProvider>();
builder.Services.AddSingleton<BlizzardUpdateHandler>();
builder.Services.AddSingleton<WarcraftClientProvider>();

builder.Services.AddHttpClient("Blizzard", x =>
{
    x.DefaultRequestHeaders.Accept.Clear();
    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
//CommonSetup.SetUpCommon(builder.Services, new CommonConfig());

//builder.Services.AddSingleton<PrintCommandHandler>();
var commanderBuilder = builder.Services.AddCommander();
//commanderBuilder.AddHandlers<PrintCommandHandler>();

builder.Services.UseRegisterAttributeScanner().RegisterFrom(typeof(CommonServices).Assembly);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateDown(0);
    runner.MigrateUp();
}

app.Services.GetRequiredService<CommonServices>().Initialize();

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
app.UseHangfireDashboard();

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

var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
await using var dbContext = dbContextFactory.CreateDbContext();
// await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

app.Run();