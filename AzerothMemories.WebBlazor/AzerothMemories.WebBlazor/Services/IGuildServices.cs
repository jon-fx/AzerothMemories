namespace AzerothMemories.WebBlazor.Services;

public interface IGuildServices : IComputeService
{
    [ComputeMethod]
    Task<GuildViewModel> TryGetGuild(Session session, int guildId);

    [ComputeMethod]
    Task<GuildMembersViewModel> TryGetGuildMembers(Session o, int guildId, int pageIndex);

    [ComputeMethod]
    Task<GuildViewModel> TryGetGuild(Session session, BlizzardRegion region, string realmSlug, string guildName);

    Task<bool> TryEnqueueUpdate(Session session, BlizzardRegion region, string realmSlug, string guildName);
}