namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostSearchPageViewModel : PersistentStateViewModel
{
    private string[] _tagStrings;
    private string _sortModeString;
    private string _currentPageString;
    private string _minTimeString;
    private string _maxTimeString;

    public PostSearchHelper PostSearchHelper { get; private set; }

    public PostSearchPageViewModel()
    {
        AddPersistentState(() => PostSearchHelper.SearchResults, x => PostSearchHelper.SetSearchResults(x), ComputeStateInternal);
    }

    public void OnParametersChanged(string[] tagStrings, string sortModeString, string currentPageString, string minTimeString, string maxTimeString)
    {
        _tagStrings = tagStrings;
        _sortModeString = sortModeString;
        _currentPageString = currentPageString;
        _minTimeString = minTimeString;
        _maxTimeString = maxTimeString;
    }

    public override async Task OnInitialized()
    {
        PostSearchHelper = new PostSearchHelper(Services);

        await base.OnInitialized();
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        await ComputeStateInternal();
    }

    private Task<SearchPostsResults> ComputeStateInternal()
    {
        return PostSearchHelper.ComputeState(_tagStrings, _sortModeString, _currentPageString, _minTimeString, _maxTimeString);
    }
}