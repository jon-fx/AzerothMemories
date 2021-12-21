using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stl.Fusion;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseUri = new Uri($"{baseUri}api/");

// Fusion services
var fusion = builder.Services.AddFusion();
var fusionClient = fusion.AddRestEaseClient((_, o) => {
    o.BaseUri = baseUri;
    o.IsLoggingEnabled = true;
    o.IsMessageLoggingEnabled = false;
});
fusionClient.ConfigureHttpClientFactory((c, name, o) => {
    var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
    var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
    o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
});
fusion.AddAuthentication().AddRestEaseClient().AddBlazor();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();