namespace AzerothMemories.WebBlazor.Components.Dialogs
{
    public sealed class AdminUserDialogViewModel : ViewModelBase
    {
        private int _accountId;


        public AdminUserDialogViewModel()
        {
            BanTimers = new[]
            {
                ("None", Duration.FromMilliseconds(0)),
                ("1 Hour", Duration.FromHours(1)),
                ("1 Day", Duration.FromDays(1)),
                ("7 Days", Duration.FromDays(7)),
                ("14 Days", Duration.FromDays(14)),
                ("28 Days", Duration.FromDays(28)),
                ("1 Year", Duration.FromDays(365)),
            };
        }

        public string ErrorMessage { get; private set; }

        public AccountViewModel AccountViewModel { get; private set; }

        public (string Text, Duration Time)[] BanTimers { get; init; }

        public string BanReasonText { get; set; }

        public void OnParametersChanged(int id)
        {
            _accountId = id;
        }

        public override async Task ComputeState(CancellationToken cancellationToken)
        {
            await base.ComputeState(cancellationToken);

            var accountViewModel = AccountViewModel;
            if (_accountId > 0)
            {
                accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountById(Session.Default, _accountId);
            }

            if (accountViewModel == null)
            {
                ErrorMessage = "Invalid Account";
                return;
            }

            AccountViewModel = accountViewModel;
        }

        public async Task ResetUsername()
        {
            await Services.ClientServices.CommandRunner.Run(new Account_TryChangeUsername(Session.Default, AccountViewModel.Id, null));
        }

        public async Task ResetAvatar()
        {
            await Services.ClientServices.CommandRunner.Run(new Account_TryChangeAvatar(Session.Default, AccountViewModel.Id, null));
        }

        public async Task ResetSocialLink(int linkId)
        {
            await Services.ClientServices.CommandRunner.Run(new Account_TryChangeSocialLink(Session.Default, AccountViewModel.Id, linkId, null));
        }

        public async Task BanUser(Duration duration)
        {
            await Services.ClientServices.CommandRunner.Run(new Admin_TryBanUser(Session.Default, AccountViewModel.Id, (long)duration.TotalMilliseconds, BanReasonText));
        }
    }
}
