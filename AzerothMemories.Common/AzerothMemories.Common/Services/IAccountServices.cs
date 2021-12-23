using RestEase;
using Stl.Fusion;

namespace AzerothMemories.Services
{
    [BasePath("account")]
    public interface IAccountServices
    {
        [ComputeMethod] [Get(nameof(TryGetAccount) + "/{accountId}")] Task<AccountViewModel> TryGetAccount([Path] long accountId);

        [Post(nameof(TryChangeUsername) + "/{newUsername}")] Task TryChangeUsername([Path] string newUsername);
    }
}