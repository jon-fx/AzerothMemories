using Stl.CommandR;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var fusion = builder.Services.AddFusion();
var fusionServer = fusion.AddWebServer();
var fusionClient = fusion.AddRestEaseClient();
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
builder.Services.AddSingleton<DatabaseProvider>();
//builder.Services.AddSingleton<QueuedUpdateHandler>();
//builder.Services.AddSingleton<WarcraftClientProvider>();

//CommonSetup.SetUpCommon(builder.Services, new CommonConfig());

//builder.Services.AddSingleton<PrintCommandHandler>();
var commanderBuilder = builder.Services.AddCommander();
//commanderBuilder.AddHandlers<PrintCommandHandler>();

fusion.AddComputeService<AccountServices>();
fusion.AddComputeService<CharacterServices>();
fusion.AddComputeService<IAccountServices, AccountServices>();

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

app.Run();