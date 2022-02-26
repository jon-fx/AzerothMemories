namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostPageViewModel : PersistentStateViewModel
{
    private string _accountString;
    private string _postIdString;
    private string _currentPageString;
    private string _focusedCommentId;

    public PostPageViewModel()
    {
        AddPersistentState(() => Helper.ErrorMessage, x => Helper.SetErrorMessage(x), () => Task.FromResult<string>(null));
        AddPersistentState(() => Helper.AccountViewModel, x => Helper.SetAccountViewModel(x), () => Helper.UpdateAccount(_accountString));
        AddPersistentState(() => Helper.PostViewModel, x => Helper.SetPostViewModel(x), () => Helper.UpdatePost(_postIdString));
        AddPersistentState(() => Helper.PostCommentPageViewModel, x => Helper.SetPostCommentPageViewModel(_currentPageString, _focusedCommentId, x), () => Helper.UpdateComments(_currentPageString, _focusedCommentId));
    }

    public override async Task OnInitialized()
    {
        Helper = new PostPageViewModelHelper(Services);

        await base.OnInitialized();
    }

    public PostPageViewModelHelper Helper { get; private set; }

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

        await Helper.UpdateAccount(_accountString);
        await Helper.UpdatePost(_postIdString);
        await Helper.UpdateComments(_currentPageString, _focusedCommentId);
    }
}