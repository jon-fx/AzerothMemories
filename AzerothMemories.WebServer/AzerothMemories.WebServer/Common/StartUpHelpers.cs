using NodaTime.Extensions;
using System.Security.Claims;

namespace AzerothMemories.WebServer.Common;

internal static class StartUpHelpers
{
    public static AuthenticationBuilder AddBlizzardAuth(this AuthenticationBuilder builder, BlizzardRegion region, CommonConfig commonConfig)
    {
        var regionInfo = region.ToInfo();
        var clientInfo = commonConfig.BlizzardClientInfo[(int)region];
        if (!clientInfo.HasValue)
        {
            return builder;
        }

        var schema = $"BattleNet-{regionInfo.Name}";
        return builder.AddBattleNet(schema, $"Battle Net - {regionInfo.Name}", options =>
        {
            options.ClaimsIssuer = schema;
            options.ClientId = clientInfo.Value.Id;
            options.ClientSecret = clientInfo.Value.Secret;
            options.CallbackPath = $"/authorization-code/callback/{regionInfo.Name.ToLower()}";
            options.TokenEndpoint = regionInfo.TokenEndpoint;
            options.AuthorizationEndpoint = regionInfo.AuthorizationEndpoint;
            options.UserInformationEndpoint = regionInfo.UserInformationEndpoint;
            //options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            //options.SaveTokens = true;
            options.Scope.Add("openid");
            options.Scope.Add("wow.profile");

            options.ClaimActions.MapJsonKey("BattleNet-Id", "id");
            options.ClaimActions.MapJsonKey("BattleNet-Tag", "battletag");

            options.Events.OnCreatingTicket += OnBlizzardCreatingTicket;
            options.Events.OnTicketReceived += OnBlizzardTicketReceived;
            options.Events.OnAccessDenied += OnBlizzardAccessDenied;
            options.Events.OnRemoteFailure += OnBlizzardRemoteFailure;
        });
    }

    private static Task OnBlizzardCreatingTicket(OAuthCreatingTicketContext context)
    {
        if (context.Identity == null)
        {
            throw new NotImplementedException();
        }

        AddTokens("BattleNet", context);

        return Task.CompletedTask;
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
    
    private static void AddTokens(string schema, OAuthCreatingTicketContext context)
    {
        if (context.Identity == null)
        {
            return;
        }

        var token = context.AccessToken ?? string.Empty;
        var tokenExpiresAt = (SystemClock.Instance.GetCurrentInstant() + context.ExpiresIn.GetValueOrDefault().ToDuration()).ToUnixTimeMilliseconds();

        context.Identity.AddClaim(new Claim($"{schema}-Token", token));
        context.Identity.AddClaim(new Claim($"{schema}-TokenExpires", tokenExpiresAt.ToString()));
    }

    public static AuthenticationBuilder AddPatreonAuth(this AuthenticationBuilder builder, CommonConfig commonConfig)
    {
        return builder;
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