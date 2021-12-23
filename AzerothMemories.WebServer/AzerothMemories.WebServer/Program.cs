var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appTempDir = FilePath.GetApplicationTempDirectory("", true);
var dbPath = appTempDir & "App.db";
builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(@"***REMOVED***");

    if (builder.Environment.IsDevelopment())
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }
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

builder.Services.AddSingleton(new CommonConfig());
builder.Services.AddSingleton<CommonServices>();
//builder.Services.AddSingleton<DatabaseProvider>();
//builder.Services.AddSingleton<QueuedUpdateHandler>();
//builder.Services.AddSingleton<WarcraftClientProvider>();

//CommonSetup.SetUpCommon(builder.Services, new CommonConfig());

//builder.Services.AddSingleton<PrintCommandHandler>();
var commanderBuilder = builder.Services.AddCommander();
//commanderBuilder.AddHandlers<PrintCommandHandler>();

builder.Services.UseRegisterAttributeScanner().RegisterFrom(typeof(CommonServices).Assembly);

var app = builder.Build();

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