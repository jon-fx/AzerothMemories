using Microsoft.JSInterop;

namespace AzerothMemories.WebBlazor.Services;

public sealed class ClientServices
{
    public ClientServices(IServiceProvider serviceProvider)
    {
        ActiveAccountServices = serviceProvider.GetRequiredService<ActiveAccountServices>();
        DialogService = serviceProvider.GetRequiredService<DialogHelperService>();
        NavigationManager = serviceProvider.GetRequiredService<NavigationManager>();
        CommandRunner = serviceProvider.GetRequiredService<UICommandRunner>();
        JsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
        TagHelpers = serviceProvider.GetRequiredService<TagHelpers>();
        TimeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        StringLocalizer = serviceProvider.GetRequiredService<IStringLocalizer<BlizzardResources>>();
        CookieHelper = serviceProvider.GetRequiredService<CookieHelper>();
    }

    public ActiveAccountServices ActiveAccountServices { get; }

    public DialogHelperService DialogService { get; }

    public NavigationManager NavigationManager { get; }

    public UICommandRunner CommandRunner { get; }

    public IJSRuntime JsRuntime { get; }

    public TagHelpers TagHelpers { get; }

    public TimeProvider TimeProvider { get; }

    public CookieHelper CookieHelper { get; }

    public IStringLocalizer<BlizzardResources> StringLocalizer { get; }

}