using Stl.Fusion.Authentication;

namespace AzerothMemories.Services
{
    public interface IAccountServices
    {
        Task<AccountViewModel> GetAccount(Session session, long accountId);
    }
}