namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountFollowPageViewModel : ViewModelBase
{
    public bool IsLoading => string.IsNullOrWhiteSpace(ErrorMessage) && AccountViewModel == null;

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public async Task ComputeState(long accountId)
    {
        var accountViewModel = AccountViewModel;
        if (accountId > 0)
        {
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountById(null, accountId);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
            return;
        }

        AccountViewModel = accountViewModel;
    }
}