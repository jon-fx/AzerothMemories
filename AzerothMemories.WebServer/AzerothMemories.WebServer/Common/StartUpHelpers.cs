using AspNet.Security.OAuth.BattleNet;
using AspNet.Security.OAuth.Patreon;
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

        return builder.AddOAuth<BattleNetAuthenticationOptions, CustomBattleNetAuthenticationHandler>("BattleNet", "Battle Net", options =>
        {
            options.ClaimsIssuer = "BattleNet";
            options.ClientId = clientInfo.Value.Id;
            options.ClientSecret = clientInfo.Value.Secret;
            options.CallbackPath = "/authorization-code/callback/battlenet";
            options.TokenEndpoint = "https://oauth.battle.net/oauth/token";
            options.AuthorizationEndpoint = "https://oauth.battle.net/oauth/authorize";
            options.UserInformationEndpoint = "https://oauth.battle.net/oauth/userinfo";

            options.Scope.Add("wow.profile");

            options.Events.OnCreatingTicket += OnBlizzardCreatingTicket;
            options.Events.OnTicketReceived += OnBlizzardTicketReceived;
            options.Events.OnAccessDenied += OnBlizzardAccessDenied;
            options.Events.OnRemoteFailure += OnBlizzardRemoteFailure;
        });
    }

    private static async Task OnBlizzardCreatingTicket(OAuthCreatingTicketContext context)
    {
        await OnCreatingTicket(context, false).ConfigureAwait(false);
    }

    private static async Task OnCreatingTicket(OAuthCreatingTicketContext context, bool requiresIsAuthenticated)
    {
        Exceptions.ThrowIf(context.Identity == null);

        var authenticationType = context.Identity.AuthenticationType;
        var id = context.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

        var session = context.HttpContext.RequestServices.GetRequiredService<ISessionResolver>().Session;
        var sessionInfo = await context.HttpContext.RequestServices.GetRequiredService<IAuth>().GetSessionInfo(session).ConfigureAwait(false);

        int? accountId = null;
        var isAuthenticated = sessionInfo != null && sessionInfo.IsAuthenticated();
        var commonServices = context.HttpContext.RequestServices.GetRequiredService<CommonServices>();
        var shouldThrowException = requiresIsAuthenticated != isAuthenticated;

        if (requiresIsAuthenticated && isAuthenticated)
        {
            var userId = sessionInfo!.UserId;
            var account = await commonServices.AccountServices.TryGetAccountRecordFusionId(userId).ConfigureAwait(false);
            if (account == null)
            {
                shouldThrowException = true;
            }
            else
            {
                accountId = account.Id;
            }
        }

        var tokenExpiresAt = (SystemClock.Instance.GetCurrentInstant() + context.ExpiresIn.GetValueOrDefault().ToDuration()).ToUnixTimeMilliseconds();
        var updateResult = await commonServices.Commander.Call(new Account_TryUpdateAuthToken(id, name, authenticationType, accountId, context.AccessToken, context.RefreshToken, tokenExpiresAt)).ConfigureAwait(false);
        if (shouldThrowException || !updateResult)
        {
            throw new CustomAuthException();
        }
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

        return builder.AddOAuth<PatreonAuthenticationOptions, CustomPatreonAuthenticationHandler>("Patreon", options =>
        {
            options.ClaimsIssuer = "Patreon";
            options.ClientId = commonConfig.PatreonClientId;
            options.ClientSecret = commonConfig.PatreonClientSecret;
            options.CallbackPath = "/authorization-code/callback/patreon";

            options.Events.OnCreatingTicket += OnPatreonCreatingTicket;
            options.Events.OnTicketReceived += OnPatreonTicketReceived;
            options.Events.OnAccessDenied += OnPatreonAccessDenied;
            options.Events.OnRemoteFailure += OnPatreonRemoteFailure;
        });
    }

    private static async Task OnPatreonCreatingTicket(OAuthCreatingTicketContext context)
    {
        await OnCreatingTicket(context, true).ConfigureAwait(false);
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