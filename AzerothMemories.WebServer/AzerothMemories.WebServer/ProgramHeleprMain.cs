using ActualLab.Fusion.EntityFramework.Npgsql;

namespace AzerothMemories.WebServer;

internal sealed class ProgramHeleprMain : ProgramHelper
{
    public ProgramHeleprMain(CommonConfig config, IServiceCollection services) : base(config, services)
    {
    }

    protected override void ConfigureDbContextFactory(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseNpgsqlHintFormatter();
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

        authenticationBuilder.AddPatreonAuth(Config);
        authenticationBuilder.AddBlizzardAuth(Config);

        authenticationBuilder.AddCookie(options =>
        {
            options.LoginPath = "/signIn";
            options.LogoutPath = "/signOut";

            options.ExpireTimeSpan = TimeSpan.FromHours(12);
            options.SlidingExpiration = false;
            options.Events.OnSigningIn = ctx =>
            {
                ctx.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(1);

                return Task.CompletedTask;
            };
        });
    }
}