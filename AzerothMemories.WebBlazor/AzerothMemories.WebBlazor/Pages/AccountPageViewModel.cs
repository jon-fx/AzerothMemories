namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountPageViewModel : ViewModelBase
{
    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public PostSearchHelper PostSearchHelper { get; private set; }

    public bool IsLoading => AccountViewModel == null || PostSearchHelper == null;

    public override async Task OnInitialized()
    {
        await base.OnInitialized();

        PostSearchHelper = new PostSearchHelper(Services);
    }

    public async Task ComputeState(long accountId, string accountUsername, string sortModeString, string currentPageString)
    {
        var accountViewModel = AccountViewModel;
        if (accountId > 0)
        {
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountById(null, accountId);
        }
        else if (!string.IsNullOrWhiteSpace(accountUsername))
        {
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountByUsername(null, accountUsername);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
            return;
        }

        AccountViewModel = accountViewModel;

        var accountTag = new PostTagInfo(PostTagType.Account, AccountViewModel.Id, AccountViewModel.Username, AccountViewModel.Avatar);
        await PostSearchHelper.ComputeState(new[] { accountTag.TagString }, sortModeString, currentPageString, null, null);
    }
}