using AzerothMemories.WebBlazor.Components;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class EditMemoryTagsPageeViewModel : ViewModelBase
{
    private PostPageViewModelHelper _postPageHelper;

    public override async Task OnInitialized()
    {
        await base.OnInitialized();

        _postPageHelper = new PostPageViewModelHelper(Services);
    }

    public PostPageViewModelHelper Helper => _postPageHelper;

    public AddMemoryComponentSharedData SharedData { get; private set; }

    public async Task ComputeState()
    {
        if (SharedData != null)
        {
            return;
        }

        if (Helper == null)
        {
            return;
        }

        var errorMessage = Helper.ErrorMessage;
        if (errorMessage != null)
        {
            return;
        }

        var accountViewModel = Helper.AccountViewModel;
        if (accountViewModel == null)
        {
            return;
        }

        var postViewModel = Helper.PostViewModel;
        if (postViewModel == null)
        {
            return;
        }

        SharedData = new AddMemoryComponentSharedData(this);
        await SharedData.InitializeAccount(() => Helper.AccountViewModel);
        await SharedData.SetPostTimeStamp(Instant.FromUnixTimeMilliseconds(postViewModel.PostTime));
        await SharedData.InitializeAchievements();
        await SharedData.OnEditingPost(postViewModel);
    }

    public async Task Submit()
    {
        if (!Services.ClientServices.ActiveAccountServices.IsAccountActiveAndCanInteract)
        {
            return;
        }

        var postViewModel = Helper.PostViewModel;
        if (postViewModel == null)
        {
            return;
        }

        if (Services.ClientServices.ActiveAccountServices.IsActiveAccount(postViewModel.AccountId) || Services.ClientServices.ActiveAccountServices.IsAdmin)
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