namespace AzerothMemories.WebBlazor.Services;

[BasePath("guild")]
public interface IGuildServices
{
    [ComputeMethod]
    [Get(nameof(TryGetGuild) + "/{guildId}")]
    Task<GuildViewModel> TryGetGuild(Session session, [Path] long guildId);

    [ComputeMethod]
    [Get(nameof(TryGetGuild) + "/{region}/{realmSlug}/{guildName}")]
    Task<GuildViewModel> TryGetGuild(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string guildName);
}