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

    public override async Task ComputeState()
    {
        await base.ComputeState();

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
        if (!Services.ActiveAccountServices.IsAccountActive)
        {
            return;
        }

        var postViewModel = Helper.PostViewModel;
        if (postViewModel == null)
        {
            return;
        }

        if (postViewModel.AccountId == Services.ActiveAccountServices.ActiveAccountId || Services.ActiveAccountServices.IsAdmin)
        {
            await SharedData.SubmitOnEditingPost(Helper.PostViewModel);
        }
    }

    public Task UserCancel()
    {
        return Task.CompletedTask;
    }
}