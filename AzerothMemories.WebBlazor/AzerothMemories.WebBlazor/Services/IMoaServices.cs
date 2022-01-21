namespace AzerothMemories.WebBlazor.Services;

public interface IMoaServices
{
    ComputeServices ComputeServices { get; }

    ActiveAccountServices ActiveAccountServices { get; }

    TagHelpers TagHelpers { get; }

    TimeProvider TimeProvider { get; }

    DialogHelperService DialogService { get; }

    IStringLocalizer<BlizzardResources> StringLocalizer { get; }

    NavigationManager NavigationManager { get; }
}