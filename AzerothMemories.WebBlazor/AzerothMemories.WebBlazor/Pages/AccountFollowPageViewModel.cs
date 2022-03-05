namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountFollowPageViewModel : ViewModelBase
{
    private int _accountId;

    public bool IsLoading => string.IsNullOrWhiteSpace(ErrorMessage) && AccountViewModel == null;

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

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
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountById(null, _accountId);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
            return;
        }

        AccountViewModel = accountViewModel;
    }
}