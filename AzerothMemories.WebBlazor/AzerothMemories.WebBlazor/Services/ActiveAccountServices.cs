namespace AzerothMemories.WebBlazor.Services
{
    public sealed class ActiveAccountServices
    {
        private readonly IAccountServices _accountServices;
        private readonly ICharacterServices _characterServices;

        public ActiveAccountServices(IAccountServices accountServices, ICharacterServices characterServices)
        {
            _accountServices = accountServices;
            _characterServices = characterServices;
        }

        public ActiveAccountViewModel AccountViewModel { get; private set; }

        public async Task ComputeState(CancellationToken cancellationToken)
        {
            AccountViewModel = await _accountServices.TryGetAccount(null, cancellationToken);
        }

        public Dictionary<long, string> GetUserTagList()
        {
            return new Dictionary<long, string>();
        }
    }
}