namespace AzerothMemories.WebBlazor.Pages;

public sealed class EditMemoryTagsPageViewModel : ViewModelBase
{
    private PostPageViewModelHelper _postPageHelper;
    private string _accountString;
    private string _postIdString;
    private string _currentPageString;
    private string _focusedCommentId;

    public override async Task OnInitialized()
    {
        _postPageHelper = new PostPageViewModelHelper(Services);

        await base.OnInitialized();
    }

    public PostPageViewModelHelper Helper => _postPageHelper;

    public AddMemoryComponentSharedData SharedData { get; private set; }

    public void OnParametersChanged(string idString, string postIdString, string currentPageString, string focusedCommentId)
    {
        _accountString = idString;
        _postIdString = postIdString;
        _currentPageString = currentPageString;
        _focusedCommentId = focusedCommentId;
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        if (_postPageHelper == null)
        {
            return;
        }

        await _postPageHelper.UpdateAccount(_accountString);
        await _postPageHelper.UpdatePost(_postIdString);
        await _postPageHelper.UpdateComments(_currentPageString, _focusedCommentId);

        var errorMessage = _postPageHelper.ErrorMessage;
        if (errorMessage != null)
        {
            return;
        }

        var accountViewModel = _postPageHelper.AccountViewModel;
        if (accountViewModel == null)
        {
            return;
        }

        var postViewModel = _postPageHelper.PostViewModel;
        if (postViewModel == null)
        {
            return;
        }

        if (SharedData == null)
        {
            SharedData = new AddMemoryComponentSharedData(this);

            await SharedData.InitializeAccount(() => _postPageHelper.AccountViewModel);
            await SharedData.SetPostTimeStamp(Instant.FromUnixTimeMilliseconds(postViewModel.PostTime));
            await SharedData.InitializeAchievements();
            await SharedData.OnEditingPost(postViewModel);
        }
    }

    public async Task Submit()
    {
        if (!Services.ClientServices.ActiveAccountServices.AccountViewModel.CanUpdateSystemTags())
        {
            return;
        }

        var postViewModel = Helper.PostViewModel;
        if (postViewModel == null)
        {
            return;
        }

        if (Services.ClientServices.ActiveAccountServices.IsActiveAccount(postViewModel.AccountId) || Services.ClientServices.ActiveAccountServices.AccountViewModel.IsAdmin())
        {
            var result = await SharedData.SubmitOnEditingPost(Helper.PostViewModel);
            if (result == AddMemoryResultCode.Success)
            {
                Services.ClientServices.NavigationManager.NavigateTo($"post/{postViewModel.AccountId}/{postViewModel.Id}");
            }
            else if (result == AddMemoryResultCode.Canceled)
            {
                UserCancel();
            }
            else
            {
                await Services.ClientServices.DialogService.ShowNotificationDialog(false, $"{result}");
            }
        }
    }

    public void UserCancel()
    {
        var postViewModel = Helper.PostViewModel;
        if (postViewModel == null)
        {
            return;
        }

        Services.ClientServices.NavigationManager.NavigateTo($"post/{postViewModel.AccountId}/{postViewModel.Id}");
    }
}