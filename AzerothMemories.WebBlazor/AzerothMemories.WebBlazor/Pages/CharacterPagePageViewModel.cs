namespace AzerothMemories.WebBlazor.Pages;

public sealed class CharacterPagePageViewModel : PersistentStateViewModel
{
    private string _idString;
    private string _region;
    private string _realm;
    private string _name;
    private string _sortModeString;
    private string _currentPageString;

    private CharacterAccountViewModel _characterAccountViewModel;

    public CharacterPagePageViewModel()
    {
        AddPersistentState(() => ErrorMessage, x => ErrorMessage = x, () => Task.FromResult<string>(null));
        AddPersistentState(() => _characterAccountViewModel, x => _characterAccountViewModel = x, UpdateCharacterAccount);
        AddPersistentState(() => PostSearchHelper.SearchResults, x => PostSearchHelper.SetSearchResults(x), UpdateSearchResults);
    }

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel => _characterAccountViewModel?.AccountViewModel;

    public CharacterViewModel CharacterViewModel => _characterAccountViewModel?.CharacterViewModel;

    public PostSearchHelper PostSearchHelper { get; private set; }

    public bool IsLoading => CharacterViewModel == null || PostSearchHelper == null;

    public void OnParametersChanged(string idString, string region, string realm, string name, string sortModeString, string currentPageString)
    {
        _idString = idString;
        _region = region;
        _realm = realm;
        _name = name;
        _sortModeString = sortModeString;
        _currentPageString = currentPageString;
    }

    public override async Task OnInitialized()
    {
        PostSearchHelper = new PostSearchHelper(Services);

        await base.OnInitialized();
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        _characterAccountViewModel = await UpdateCharacterAccount();

        await UpdateSearchResults();
    }

    private async Task<CharacterAccountViewModel> UpdateCharacterAccount()
    {
        int.TryParse(_idString, out var id);

        if (id > 0)
        {
            var results = await Services.ComputeServices.CharacterServices.TryGetCharacter(Session.Default, id);
            if (results == null || results.CharacterViewModel == null)
            {
                ErrorMessage = "Invalid Character";

                return null;
            }

            return results;
        }

        if (string.IsNullOrWhiteSpace(_region))
        {
            ErrorMessage = "Invalid Region";
            return null;
        }

        if (string.IsNullOrWhiteSpace(_realm))
        {
            ErrorMessage = "Invalid Realm";
            return null;
        }

        if (string.IsNullOrWhiteSpace(_name))
        {
            ErrorMessage = "Invalid Name";
            return null;
        }

        var regionInfo = BlizzardRegionInfo.AllByName.Values.FirstOrDefault(x => x.TwoLetters.ToLowerInvariant() == _region.ToLowerInvariant());
        if (regionInfo == null)
        {
            ErrorMessage = "Invalid Region";
            return null;
        }

        if (!Services.ClientServices.TagHelpers.GetRealmId(_realm, out _) && !Services.ClientServices.TagHelpers.GetRealmSlug($"{regionInfo.TwoLetters}-{_realm}", out _realm))
        {
            ErrorMessage = "Invalid Realm";
            return null;
        }

        var viewModel = await Services.ComputeServices.CharacterServices.TryGetCharacter(Session.Default, regionInfo.Region, _realm, _name);
        if (viewModel == null || viewModel.CharacterViewModel == null)
        {
            ErrorMessage = "Invalid Character";
            return null;
        }

        return viewModel;
    }

    private Task<SearchPostsResults> UpdateSearchResults()
    {
        if (CharacterViewModel == null)
        {
            return Task.FromResult(new SearchPostsResults());
        }

        var characterTag = new PostTagInfo(PostTagType.Character, CharacterViewModel.Id, CharacterViewModel.Name, CharacterViewModel.AvatarLinkWithFallBack);
        return PostSearchHelper.ComputeState(new[] { characterTag.TagString }, _sortModeString, _currentPageString, null, null);
    }
}