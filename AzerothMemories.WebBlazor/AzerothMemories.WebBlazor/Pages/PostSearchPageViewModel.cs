namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostSearchPageViewModel : PersistentStateViewModel, IViewModel<PostSearchPageViewModel>
{
    private string[] _tagStrings;
    private string _sortModeString;
    private string _currentPageString;
    private string _minTimeString;
    private string _maxTimeString;

    public PostSearchHelper PostSearchHelper { get; }

    public PostSearchPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        PostSearchHelper = new PostSearchHelper(Services);

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

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        await ComputeStateInternal();
    }

    private Task<SearchPostsResults> ComputeStateInternal()
    {
        return PostSearchHelper.ComputeState(_tagStrings, _sortModeString, _currentPageString, _minTimeString, _maxTimeString);
    }

    public static PostSearchPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new PostSearchPageViewModel(services, onViewModelChanged);
    }
}