using Stl.Fusion.Client.Interception;
using Stl.Fusion.Diagnostics;
using Stl.Fusion.Extensions;
using Stl.Fusion.Internal;
using Stl.OS;
using Stl.Time;

namespace AzerothMemories.WebBlazor;

public static class ProgramEx
{
    public static void Initialize(IServiceCollection services)
    {
#if !DEBUG
        Stl.Interception.Interceptors.InterceptorBase.Options.Defaults.IsValidationEnabled = false;
#endif

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
        services.AddSingleton<MarkdownServices>();

        services.AddSingleton<TagHelpers>();
        services.AddSingleton<TimeProvider>();

        services.AddScoped<ClientServices>();
        services.AddScoped<ActiveAccountServices>();
        services.AddScoped<DialogHelperService>();

        var fusion = services.AddFusion();
        fusion.AddComputedGraphPruner(_ => new ComputedGraphPruner.Options { CheckPeriod = TimeSpan.FromSeconds(30) });
        fusion.AddFusionTime();

        services.AddScoped<IUpdateDelayer>(c => new UpdateDelayer(c.UIActionTracker(), 0.5));

        services.AddHostedService(c =>
        {
            var isWasm = OSInfo.IsWebAssembly;
            return new FusionMonitor(c)
            {
                SleepPeriod = isWasm ? TimeSpan.Zero : TimeSpan.FromMinutes(1).ToRandom(0.25),
                CollectPeriod = TimeSpan.FromSeconds(isWasm ? 3 : 60),
                AccessFilter = isWasm ? static computed => computed.Input.Function is IClientComputeMethodFunction : static computed => true,
                AccessStatisticsPreprocessor = StatisticsPreprocessor,
                RegistrationStatisticsPreprocessor = StatisticsPreprocessor,
            };

            static void StatisticsPreprocessor(Dictionary<string, (int, int)> stats)
            {
                foreach (var key in stats.Keys.ToList())
                {
                    if (key.Contains(".Pseudo"))
                    {
                        stats.Remove(key);
                    }

                    if (key.StartsWith("FusionTime."))
                    {
                        stats.Remove(key);
                    }
                }
            }
        });
    }
}