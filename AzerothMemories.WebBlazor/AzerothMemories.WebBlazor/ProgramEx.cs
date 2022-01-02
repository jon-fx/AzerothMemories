using AzerothMemories.WebBlazor.Pages;
using AzerothMemories.WebBlazor.Services;

namespace AzerothMemories.WebBlazor
{
    public static class ProgramEx
    {
        public static void Initialize(IServiceCollection services)
        {
            services.AddMudServices();
            services.AddLocalization();

            services.AddSingleton<TimeProvider>();
            services.AddSingleton<AccountManagePageViewModel>();
        }
    }
}