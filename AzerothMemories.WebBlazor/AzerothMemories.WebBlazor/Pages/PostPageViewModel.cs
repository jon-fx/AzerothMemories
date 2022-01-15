namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostPageViewModel : ViewModelBase
{
    private PostPageViewModelHelper _postPageHelper;

    public override async Task OnInitialized()
    {
        await base.OnInitialized();

        _postPageHelper = new PostPageViewModelHelper(Services);
    }

    public PostPageViewModelHelper Helper => _postPageHelper;
}