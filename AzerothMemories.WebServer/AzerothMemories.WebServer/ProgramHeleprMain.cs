namespace AzerothMemories.WebServer;

internal sealed class ProgramHeleprMain : ProgramHelper
{
    public ProgramHeleprMain(CommonConfig config, IServiceCollection services) : base(config, services)
    {
    }

    protected override void ConfigureDbContextFactory(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseNpgsql(Config.DatabaseConnectionString, o => o.UseNodaTime());
    }

    protected override void OnInitializeAuth()
    {
        var authenticationBuilder = Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        authenticationBuilder.AddPatreonAuth();
        authenticationBuilder.AddBlizzardAuth(BlizzardRegion.Europe, Config);
        authenticationBuilder.AddBlizzardAuth(BlizzardRegion.Taiwan, Config);
        authenticationBuilder.AddBlizzardAuth(BlizzardRegion.Korea, Config);
        authenticationBuilder.AddBlizzardAuth(BlizzardRegion.UnitedStates, Config);

        authenticationBuilder.AddCookie(options =>
        {
            options.LoginPath = "/signIn";
            options.LogoutPath = "/signOut";

            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            options.Events.OnSigningIn = ctx =>
            {
                ctx.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(14);

                return Task.CompletedTask;
            };
        });
    }
}