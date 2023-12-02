using AspNet.Security.OAuth.BattleNet;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace AzerothMemories.WebServer.Common;

internal sealed class CustomBattleNetAuthenticationHandler : BattleNetAuthenticationHandler
{
    public CustomBattleNetAuthenticationHandler(IOptionsMonitor<BattleNetAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
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