using AspNet.Security.OAuth.Patreon;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace AzerothMemories.WebServer.Common;

internal sealed class CustomPatreonAuthenticationHandler : PatreonAuthenticationHandler
{
    public CustomPatreonAuthenticationHandler(IOptionsMonitor<PatreonAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        try
        {
            return await base.HandleRemoteAuthenticateAsync().ConfigureAwait(false);
        }
        catch (CustomAuthException e)
        {
            return HandleRequestResult.Fail(e);
        }
    }
}