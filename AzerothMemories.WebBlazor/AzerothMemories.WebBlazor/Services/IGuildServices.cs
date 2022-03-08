namespace AzerothMemories.WebBlazor.Services;

[BasePath("guild")]
public interface IGuildServices
{
    [ComputeMethod]
    [Get(nameof(TryGetGuild) + "/{guildId}")]
    Task<GuildViewModel> TryGetGuild(Session session, [Path] int guildId);

    [ComputeMethod]
    [Get(nameof(TryGetGuildMembers) + "/{guildId}/{pageIndex}")]
    Task<GuildMembersViewModel> TryGetGuildMembers(Session o, [Path] int guildId, [Path] int pageIndex);

    [ComputeMethod]
    [Get(nameof(TryGetGuild) + "/{region}/{realmSlug}/{guildName}")]
    Task<GuildViewModel> TryGetGuild(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string guildName);

    [Post(nameof(TryEnqueueUpdate) + "/{region}/{realmSlug}/{guildName}")]
    Task<bool> TryEnqueueUpdate(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string guildName);
}