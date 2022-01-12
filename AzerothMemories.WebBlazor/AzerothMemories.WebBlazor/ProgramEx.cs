namespace AzerothMemories.WebBlazor;

public static class ProgramEx
{
    public static void Initialize(IServiceCollection services)
    {
        services.AddMudServices();
        services.AddLocalization();

        services.AddSingleton<TagHelpers>();
        services.AddSingleton<TimeProvider>();
        services.AddSingleton<ActiveAccountServices>();

        services.AddScoped<DialogHelperService>();
    }
}