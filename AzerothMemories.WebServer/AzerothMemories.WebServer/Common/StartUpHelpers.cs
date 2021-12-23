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

            options.ClaimActions.MapJsonKey("BlizzardId", "id");
            options.ClaimActions.MapJsonKey("BattleTag", "battletag");

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

    private static async Task OnCreatingTicket(OAuthCreatingTicketContext context)
    {
        if (context.Identity == null)
        {
            throw new NotImplementedException();
        }

        long moaId = -1;
        var moaIdClaim = context.Identity.FindFirst("Id");
        if (moaIdClaim != null)
        {
            if (!long.TryParse(moaIdClaim.Value, out moaId))
            {
                throw new NotImplementedException();
            }
        }

        if (moaId >= 0)
        {
            throw new NotImplementedException();
        }

        var blizzardIdClaim = context.Identity.FindFirst("BlizzardId");
        if (blizzardIdClaim == null)
        {
            throw new NotImplementedException();
        }

        if (!long.TryParse(blizzardIdClaim.Value, out var blizzardId))
        {
            throw new NotImplementedException();
        }

        var battleTagClaim = context.Identity.FindFirst("BattleTag");
        if (battleTagClaim == null)
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
        var accountRef = MoaRef.GetAccountRef(blizzardRegion, blizzardId);
        //var accountId = await context.HttpContext.RequestServices.GetRequiredService<AccountServices>().GetAccountId(accountRef.Full);

        //context.Identity.AddClaim(new Claim("MoaId", accountId.ToString(CultureInfo.InvariantCulture)));
        //context.Identity.AddClaim(new Claim("MoaRef", accountRef.Full));

        //await context.HttpContext.RequestServices.GetRequiredService<AccountServices>().OnLogin(accountId, battleTagClaim.Value, token, tokenExpiresAt);
    }
}