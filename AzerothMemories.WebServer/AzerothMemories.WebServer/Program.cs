using AzerothMemories.WebBlazor;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework.Npgsql;
using Stl.Fusion.Server.Authentication;
using Stl.Fusion.Server.Controllers;
using System.Net.Http.Headers;
using System.Text;

var config = new CommonConfig();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
    //if (Env.IsDevelopment()) {
    logging.AddFilter("Microsoft", LogLevel.Warning);
    logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
    //logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
    //}
});

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

ProgramEx.Initialize(builder.Services);

builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(config.DatabaseConnectionString);
});
builder.Services.AddTransient(c => new DbOperationScope<AppDbContext>(c)
{
    IsolationLevel = System.Data.IsolationLevel.Serializable,
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
});
builder.Services.AddHangfireServer(options =>
{
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

    // This controls the expiration time stored in the cookie itself
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    // And this controls when the browser forgets the cookie
    options.Events.OnSigningIn = ctx =>
    {
        ctx.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(28);
        return Task.CompletedTask;
    };
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

builder.Services.UseRegisterAttributeScanner().RegisterFrom(typeof(CommonServices).Assembly);

var app = builder.Build();
app.Services.GetRequiredService<CommonServices>().Initialize();
app.Services.GetRequiredService<ComputeServices>().Initialize();

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