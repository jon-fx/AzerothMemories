namespace AzerothMemories.WebServer.Services
{
    public static class AccountRecordToViewModels
    {
        public static void PopulateViewModel(AccountViewModel accountViewModel, AccountRecord accountRecord)
        {
            accountViewModel.Id = accountRecord.Id;
            accountViewModel.Avatar = accountRecord.Avatar;
            accountViewModel.Username = accountRecord.Username;
            accountViewModel.BattleTag = accountRecord.BattleTag;
            accountViewModel.CreatedDateTime = accountRecord.CreatedDateTime;
        }

        public static AccountViewModel CreateAccountViewModel(this AccountRecord accountRecord)
        {
            var viewModel = new ActiveAccountViewModel();

            PopulateViewModel(viewModel, accountRecord);

            return viewModel;
        }

        public static ActiveAccountViewModel CreateActiveAccountViewModel(this AccountRecord accountRecord)
        {
            var viewModel = new ActiveAccountViewModel();

            PopulateViewModel(viewModel, accountRecord);

            return viewModel;
        }
    }
}