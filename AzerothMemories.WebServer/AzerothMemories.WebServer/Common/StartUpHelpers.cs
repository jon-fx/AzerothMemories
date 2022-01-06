using NodaTime.Extensions;
using System.Security.Claims;

namespace AzerothMemories.WebServer.Common;

internal static class StartUpHelpers
{
    public static AuthenticationBuilder AddOAuth(this AuthenticationBuilder builder, BlizzardRegion region)
    {
        var regionInfo = region.ToInfo();
        var schema = $"BattleNet-{regionInfo.Name}";
        return builder.AddBattleNet(schema, $"Battle Net - {regionInfo.Name}", options =>
        {
            options.ClaimsIssuer = schema;
            options.ClientId = "***REMOVED***";
            options.ClientSecret = "***REMOVED***";
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

            options.Events.OnCreatingTicket += OnCreatingTicket;
            options.Events.OnTicketReceived += OnTicketReceived;
            options.Events.OnAccessDenied += OnAccessDenied;
            options.Events.OnRemoteFailure += OnRemoteFailure;
        });
    }

    private static Task OnTicketReceived(TicketReceivedContext arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnAccessDenied(AccessDeniedContext arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnRemoteFailure(RemoteFailureContext arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnCreatingTicket(OAuthCreatingTicketContext context)
    {
        if (context.Identity == null)
        {
            throw new NotImplementedException();
        }

        var regionStr = context.Scheme.Name.Replace("BattleNet-", "");
        var blizzardRegion = BlizzardRegionExt.FromName(regionStr);
        var token = context.AccessToken;
        if (token == null)
        {
            throw new NotImplementedException();
        }

        var tokenExpiresAt = (SystemClock.Instance.GetCurrentInstant() + context.ExpiresIn.GetValueOrDefault().ToDuration()).ToUnixTimeMilliseconds();

        context.Identity.AddClaim(new Claim("BattleNet-Token", token));
        context.Identity.AddClaim(new Claim("BattleNet-TokenExpires", tokenExpiresAt.ToString()));
        context.Identity.AddClaim(new Claim("BattleNet-Region", blizzardRegion.ToString()));

        return Task.CompletedTask;
    }
}