namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class AccountManagePageViewModel
    {
        private readonly IAccountServices _accountServices;

        public AccountManagePageViewModel(IAccountServices accountServices)
        {
            _accountServices = accountServices;
        }

        public async Task ComputeState(CancellationToken cancellationToken)
        {
            AccountViewModel = await _accountServices.TryGetAccount(null, cancellationToken);
        }

        public ActiveAccountViewModel AccountViewModel { get; private set; }
    }
}