namespace AzerothMemories.WebBlazor.Pages;

public sealed class CharacterPagePageViewModel : PersistentStateViewModel, IViewModel<CharacterPagePageViewModel>, IPageHeaderInfoProvider
{
    private string? _idString;
    private string? _region;
    private string? _realm;
    private string? _name;
    private string? _sortModeString;
    private string? _currentPageString;

    private CharacterAccountViewModel? _characterAccountViewModel;

    public CharacterPagePageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        PostSearchHelper = new PostSearchHelper(Services);

        AddPersistentState(() => ErrorMessage, x => ErrorMessage = x, () => Task.FromResult<string?>(null));
        AddPersistentState(() => _characterAccountViewModel, x => _characterAccountViewModel = x, UpdateCharacterAccount);
        AddPersistentState(() => PostSearchHelper.SearchResults, x => PostSearchHelper.SetSearchResults(x), UpdateSearchResults!);
    }

    public string? ErrorMessage { get; private set; }

    public AccountViewModel? AccountViewModel => _characterAccountViewModel?.AccountViewModel;

    public CharacterViewModel? CharacterViewModel => _characterAccountViewModel?.CharacterViewModel;

    public PostSearchHelper PostSearchHelper { get; }

    public bool IsLoading => CharacterViewModel == null;

    public void OnParametersChanged(string? idString, string? region, string? realm, string? name, string? sortModeString, string? currentPageString)
    {
        _idString = idString;
        _region = region;
        _realm = realm;
        _name = name;
        _sortModeString = sortModeString;
        _currentPageString = currentPageString;
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        _characterAccountViewModel = await UpdateCharacterAccount();

        await UpdateSearchResults();
    }

    private async Task<CharacterAccountViewModel?> UpdateCharacterAccount()
    {
        int.TryParse(_idString, out var id);

        ErrorMessage = null;
        CharacterAccountViewModel? viewModel = null;

        if (id > 0)
        {
            viewModel = await Services.ComputeServices.CharacterServices.TryGetCharacter(Session.Default, id);
        }
        else
        {
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

            if (!BlizzardRegionInfo.AllByTwoLetters.TryGetValue(_region, out var regionInfo))
            {
                ErrorMessage = "Invalid Region";
                return null;
            }

            if (!Services.ClientServices.TagHelpers.GetRealmId(_realm, out _) && !Services.ClientServices.TagHelpers.GetRealmSlug($"{regionInfo.TwoLettersLower}-{_realm}", out _realm))
            {
                ErrorMessage = "Invalid Realm";
                return null;
            }

            viewModel = await Services.ComputeServices.CharacterServices.TryGetCharacter(Session.Default, regionInfo.Region, _realm, _name);
        }

        if (viewModel == null || viewModel.CharacterViewModel == null)
        {
            ErrorMessage = "Invalid Character";
            return null;
        }

        var characterStatus = viewModel.CharacterViewModel.CharacterStatus;
        if (characterStatus == CharacterStatus2.Deleted || characterStatus == CharacterStatus2.DeletePending)
        {
            ErrorMessage = "Character Deleted";
            return null;
        }

        if (characterStatus == CharacterStatus2.RenamedOrTransferred)
        {
            ErrorMessage = "Character Renamed or Transferred";
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

        var characterTag = new PostTagInfo(PostTagType.Character, CharacterViewModel.Id, CharacterViewModel.Name, CharacterViewModel.GetAvatarLinkWithFallBack());
        return PostSearchHelper.ComputeState([characterTag.TagString], _sortModeString, _currentPageString, null, null);
    }

    public string GetPageTitle()
    {
        return $"{CharacterViewModel.GetDisplayName()}'s Memories of Azeroth";
    }

    public string GetPageDescription()
    {
        var characterDesc = CharacterViewModel.GetDescription(Services.ClientServices.BlizzardStringLocalizer);
        var accountDesc = string.Empty;
        if (AccountViewModel != null)
        {
            accountDesc = $" Their linked account is {AccountViewModel.GetDisplayName()}. {AccountViewModel.GetDescription(Services.ClientServices.BlizzardStringLocalizer)}";
        }

        return $"A collection of Memories of Azeroth from the character {characterDesc}.{accountDesc}";
    }

    public string? GetPageImage()
    {
        return CharacterViewModel.GetAvatarLinkWithFallBack();
    }

    public string GetPageImageAlt()
    {
        return $"{CharacterViewModel.GetDisplayName()}'s Avatar";
    }

    public string? GetCanonicalLink()
    {
        if (CharacterViewModel == null)
        {
            return null;
        }

        return $"character/{CharacterViewModel.Id}";
    }

    public static CharacterPagePageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new CharacterPagePageViewModel(services, onViewModelChanged);
    }
}