namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class GuildController : ControllerBase, IGuildServices
{
    private readonly CommonServices _commonServices;

    public GuildController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet("{guildId}"), Publish]
    public Task<GuildViewModel> TryGetGuild(Session session, [FromRoute] long guildId)
    {
        return _commonServices.GuildServices.TryGetGuild(session, guildId);
    }

    [HttpGet("{region}/{realmSlug}/{guildName}"), Publish]
    public Task<GuildViewModel> TryGetGuild(Session session, [FromRoute] BlizzardRegion region, [FromRoute] string realmSlug, [FromRoute] string guildName)
    {
        return _commonServices.GuildServices.TryGetGuild(session, region, realmSlug, guildName);
    }

    [HttpPost("{region}/{realmSlug}/{guildName}")]
    public Task<bool> TryEnqueueUpdate(Session session, [FromRoute] BlizzardRegion region, [FromRoute] string realmSlug, [FromRoute] string guildName)
    {
        return _commonServices.GuildServices.TryEnqueueUpdate(session, region, realmSlug, guildName);
    }
}