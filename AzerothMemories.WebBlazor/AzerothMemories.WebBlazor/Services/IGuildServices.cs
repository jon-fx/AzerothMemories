namespace AzerothMemories.WebBlazor.Services;

public interface IGuildServices : IComputeService
{
    [ComputeMethod]
    Task<GuildViewModel?> TryGetGuild(Session session, int guildId);

    [ComputeMethod]
    Task<GuildMembersViewModel> TryGetGuildMembers(Session session, int guildId, int pageIndex);

    [ComputeMethod]
    Task<GuildViewModel?> TryGetGuild(Session session, BlizzardRegion region, BlizzardRealmVersion realmVersion, string? realmSlug, string? guildName);
}