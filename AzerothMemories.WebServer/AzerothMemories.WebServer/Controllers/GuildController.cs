#if DEBUG

namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class GuildController : ControllerBase, IGuildServices
{
    private readonly CommonServices _commonServices;

    public GuildController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet("{guildId:int}")]
    public Task<GuildViewModel?> TryGetGuild(Session session, [FromRoute] int guildId)
    {
        return _commonServices.GuildServices.TryGetGuild(session, guildId);
    }

    [HttpGet("{guildId:int}/{pageIndex:int}")]
    public Task<GuildMembersViewModel> TryGetGuildMembers(Session session, int guildId, int pageIndex)
    {
        return _commonServices.GuildServices.TryGetGuildMembers(session, guildId, pageIndex);
    }

    [HttpGet("{region}/{realmSlug}/{guildName}")]
    public Task<GuildViewModel?> TryGetGuild(Session session, [FromRoute] BlizzardRegion region, [FromRoute] string? realmSlug, [FromRoute] string? guildName)
    {
        return _commonServices.GuildServices.TryGetGuild(session, region, realmSlug, guildName);
    }
}

#endif