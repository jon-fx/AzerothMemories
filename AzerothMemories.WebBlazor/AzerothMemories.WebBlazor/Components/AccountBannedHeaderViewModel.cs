namespace AzerothMemories.WebBlazor.Components;

public sealed class AccountBannedHeaderViewModel : ViewModelBase, IViewModel<AccountBannedHeaderViewModel>
{
    public AccountBannedHeaderViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
    }

    public static AccountBannedHeaderViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new AccountBannedHeaderViewModel(services, onViewModelChanged);
    }
}