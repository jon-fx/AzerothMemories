[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AzerothMemories.WebServer.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AzerothMemories.WebServer.TestsFake")]

var config = new CommonConfig();
var builder = WebApplication.CreateBuilder(args);
var helper = new ProgramHeleprMain(config, builder.Services);

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

helper.Initialize();

builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<BlizzardUpdateHostedService>();

builder.Services.AddServerSideBlazor(o => o.DetailedErrors = true);
helper.FusionAuthentication.AddBlazor(_ => { });

var app = builder.Build();
helper.Configure(app.Services);
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

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
};

app.UseWebSockets(webSocketOptions);
app.UseFusionSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapBlazorHub();
//    endpoints.MapFusionWebSocketServer();
//    endpoints.MapControllers();
//    //endpoints.MapRazorPages();
//    endpoints.MapFallbackToPage("/_Host");
//});

app.MapBlazorHub();
app.MapFusionWebSocketServer();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();