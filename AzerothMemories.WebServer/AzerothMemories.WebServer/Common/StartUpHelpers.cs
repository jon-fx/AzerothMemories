using NodaTime.Extensions;
using System.Security.Claims;

namespace AzerothMemories.WebServer.Common;

internal static class StartUpHelpers
{
    public static AuthenticationBuilder AddBlizzardAuth(this AuthenticationBuilder builder, CommonConfig commonConfig)
    {
        var clientInfo = commonConfig.BlizzardClientInfo[(int)BlizzardRegion.Europe];
        if (!clientInfo.HasValue)
        {
            return builder;
        }

        return builder.AddBattleNet("BattleNet", "Battle Net", options =>
        {
            options.ClaimsIssuer = "BattleNet";
            options.ClientId = clientInfo.Value.Id;
            options.ClientSecret = clientInfo.Value.Secret;
            options.CallbackPath = "/authorization-code/callback/battlenet";
            options.TokenEndpoint = "https://oauth.battle.net/oauth/token";
            options.AuthorizationEndpoint = "https://oauth.battle.net/oauth/authorize";
            options.UserInformationEndpoint = "https://oauth.battle.net/oauth/userinfo";

            //options.SaveTokens = false;
            //options.Scope.Add("openid");
            options.Scope.Add("wow.profile");

            options.Events.OnCreatingTicket += OnBlizzardCreatingTicket;
            options.Events.OnTicketReceived += OnBlizzardTicketReceived;
            options.Events.OnAccessDenied += OnBlizzardAccessDenied;
            options.Events.OnRemoteFailure += OnBlizzardRemoteFailure;
        });
    }

    private static async Task OnBlizzardCreatingTicket(OAuthCreatingTicketContext context)
    {
        await OnCreatingTicket(context).ConfigureAwait(false);
    }

    private static async Task OnCreatingTicket(OAuthCreatingTicketContext context)
    {
        Exceptions.ThrowIf(context.Identity == null);

        var authenticationType = context.Identity.AuthenticationType;
        var id = context.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

        var accountServices = context.HttpContext.RequestServices.GetRequiredService<AccountServices>();
        await accountServices.TryUpdateAuthToken(new Account_TryUpdateAuthToken
        {
            Id = id,
            Name = name,
            Type = authenticationType,
            AccessToken = context.AccessToken,
            RefreshToken = context.RefreshToken,
            TokenExpiresAt = (SystemClock.Instance.GetCurrentInstant() + context.ExpiresIn.GetValueOrDefault().ToDuration()).ToUnixTimeMilliseconds()
        }).ConfigureAwait(false);
    }

    private static Task OnBlizzardTicketReceived(TicketReceivedContext arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnBlizzardAccessDenied(AccessDeniedContext arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnBlizzardRemoteFailure(RemoteFailureContext arg)
    {
        return Task.CompletedTask;
    }

    public static AuthenticationBuilder AddPatreonAuth(this AuthenticationBuilder builder, CommonConfig commonConfig)
    {
        if (commonConfig.PatreonClientId == null || commonConfig.PatreonClientSecret == null)
        {
            return builder;
        }

        return builder.AddPatreon("Patreon", options =>
        {
            options.ClaimsIssuer = "Patreon";
            options.ClientId = commonConfig.PatreonClientId;
            options.ClientSecret = commonConfig.PatreonClientSecret;
            options.CallbackPath = "/authorization-code/callback/patreon";

            //options.SaveTokens = false;
            //options.ClaimActions.MapJsonKey("Patreon-Id", "id");
            //options.ClaimActions.MapJsonSubKey("Patreon-Tag", "attributes", "full_name");

            options.Events.OnCreatingTicket += OnPatreonCreatingTicket;
            options.Events.OnTicketReceived += OnPatreonTicketReceived;
            options.Events.OnAccessDenied += OnPatreonAccessDenied;
            options.Events.OnRemoteFailure += OnPatreonRemoteFailure;
        });
    }

    private static async Task OnPatreonCreatingTicket(OAuthCreatingTicketContext context)
    {
        await OnCreatingTicket(context).ConfigureAwait(false);
    }

    private static Task OnPatreonTicketReceived(TicketReceivedContext arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnPatreonAccessDenied(AccessDeniedContext arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnPatreonRemoteFailure(RemoteFailureContext arg)
    {
        return Task.CompletedTask;
    }

    public static HeaderPolicyCollection GetHeaderPolicyCollection()
    {
        var policy = new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddXssProtectionBlock()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .RemoveServerHeader()
            .AddCrossOriginOpenerPolicy(builder => builder.SameOriginAllowPopups())
            .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
            //.AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
            .AddContentSecurityPolicyReportOnly(builder =>
            {
            })
            .AddPermissionsPolicy(builder =>
            {
                builder.AddAccelerometer().None();
                builder.AddAutoplay().None();
                builder.AddCamera().None();
                builder.AddEncryptedMedia().None();
                builder.AddFullscreen().All();
                builder.AddGeolocation().None();
                builder.AddGyroscope().None();
                builder.AddMagnetometer().None();
                builder.AddMicrophone().None();
                builder.AddMidi().None();
                builder.AddPayment().None();
                builder.AddPictureInPicture().None();
                builder.AddSyncXHR().None();
                builder.AddUsb().None();
            });

        //#if DEBUG
        policy.AddStrictTransportSecurityMaxAgeIncludeSubDomains();
        //#endif

        return policy;
    }
}