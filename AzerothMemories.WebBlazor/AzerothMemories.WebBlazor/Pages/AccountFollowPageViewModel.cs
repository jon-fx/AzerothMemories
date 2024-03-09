namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountFollowPageViewModel : ViewModelBase, IViewModel<AccountFollowPageViewModel>
{
    private int _accountId;

    public bool IsLoading => string.IsNullOrWhiteSpace(ErrorMessage) && AccountViewModel == null;

    private AccountFollowPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
    }

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
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountById(Session.Default, _accountId);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
        }
        else
        {
            ErrorMessage = null;
        }

        AccountViewModel = accountViewModel;
    }

    public static AccountFollowPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new AccountFollowPageViewModel(services, onViewModelChanged);
    }
}