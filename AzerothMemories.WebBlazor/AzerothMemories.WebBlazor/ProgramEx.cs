using Blazor.Analytics;

namespace AzerothMemories.WebBlazor;

public static class ProgramEx
{
    public static void Initialize(IServiceCollection services)
    {
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;

            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.NewestOnTop = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 10000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });

        services.AddLocalization();

        services.AddSingleton<ComputeServices>();

        services.AddSingleton<TagHelpers>();
        services.AddSingleton<TimeProvider>();
        services.AddSingleton<CookieHelper>();

        services.AddScoped<ClientServices>();
        services.AddScoped<ActiveAccountServices>();
        services.AddScoped<DialogHelperService>();

        services.AddGoogleAnalytics("G-G4QYXHL15Y");
    }
}