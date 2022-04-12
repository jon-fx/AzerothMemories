using System.Collections.Immutable;
using System.Security.Claims;
using Stl.Fusion.Server.Authentication;
using Stl.Fusion.Server.Internal;
using Stl.Time;

namespace AzerothMemories.WebServer.Common;

internal sealed class CustomServerAuthHelper : ServerAuthHelper
{
    private readonly AccountServices _accountServices;

    public CustomServerAuthHelper(Options settings, IAuth auth, IAuthBackend authBackend, ISessionResolver sessionResolver, AuthSchemasCache authSchemasCache, MomentClockSet clocks, ICommander commander, AccountServices accountServices) : base(settings, auth, authBackend, sessionResolver, authSchemasCache, clocks)
    {
        _accountServices = accountServices;
    }

    protected override bool IsSameUser(User user, ClaimsPrincipal httpUser, string schema)
    {
        var result = base.IsSameUser(user, httpUser, schema);

        return false;
    }
}