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
        ScrollManager = serviceProvider.GetRequiredService<IScrollManager>();
        TagHelpers = serviceProvider.GetRequiredService<TagHelpers>();
        TimeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        BlizzardStringLocalizer = serviceProvider.GetRequiredService<IStringLocalizer<BlizzardResources>>();
        PersistentComponentState = serviceProvider.GetRequiredService<PersistentComponentState>();
    }

    public ActiveAccountServices ActiveAccountServices { get; }

    public DialogHelperService DialogService { get; }

    public NavigationManager NavigationManager { get; }

    public UICommandRunner CommandRunner { get; }

    public IJSRuntime JsRuntime { get; }

    public IScrollManager ScrollManager { get; }

    public TagHelpers TagHelpers { get; }

    public TimeProvider TimeProvider { get; }

    public IStringLocalizer<BlizzardResources> BlizzardStringLocalizer { get; }

    public PersistentComponentState PersistentComponentState { get; }
}