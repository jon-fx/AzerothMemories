namespace AzerothMemories.WebBlazor.Pages;

public sealed class GuildPageViewModel : PersistentStateViewModel
{
    private string _idString;
    private string _region;
    private string _realm;
    private string _name;
    private string _sortModeString;
    private string _currentPageString;

    public GuildPageViewModel()
    {
        AddPersistentState(() => ErrorMessage, x => ErrorMessage = x, () => Task.FromResult<string>(null));
        AddPersistentState(() => GuildViewModel, x => GuildViewModel = x, UpdateGuildViewModel);
        AddPersistentState(() => PostSearchHelper.SearchResults, x => PostSearchHelper.SetSearchResults(x), UpdateSearchResults);
    }

    public string ErrorMessage { get; private set; }

    public GuildViewModel GuildViewModel { get; private set; }

    public PostSearchHelper PostSearchHelper { get; private set; }

    public bool IsLoading => GuildViewModel == null || PostSearchHelper == null;

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

        GuildViewModel = await UpdateGuildViewModel();

        await UpdateSearchResults();
    }

    private async Task<GuildViewModel> UpdateGuildViewModel()
    {
        int.TryParse(_idString, out var id);
        GuildViewModel guildViewModel;

        if (id > 0)
        {
            guildViewModel = await Services.ComputeServices.GuildServices.TryGetGuild(Session.Default, id);
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

            var regionInfo = BlizzardRegionInfo.AllByName.Values.FirstOrDefault(x => string.Equals(x.TwoLettersLower, _region, StringComparison.InvariantCultureIgnoreCase));
            if (regionInfo == null)
            {
                ErrorMessage = "Invalid Region";
                return null;
            }

            if (!Services.ClientServices.TagHelpers.GetRealmId(_realm, out _) && !Services.ClientServices.TagHelpers.GetRealmSlug($"{regionInfo.TwoLettersLower}-{_realm}", out _realm))
            {
                ErrorMessage = "Invalid Realm";
                return null;
            }

            guildViewModel = await Services.ComputeServices.GuildServices.TryGetGuild(Session.Default, regionInfo.Region, _realm, _name);
        }

        if (guildViewModel == null)
        {
            ErrorMessage = "Invalid Guild";
            return null;
        }

        return guildViewModel;
    }

    private Task<SearchPostsResults> UpdateSearchResults()
    {
        if (GuildViewModel == null)
        {
            return Task.FromResult(new SearchPostsResults());
        }

        var guildTag = new PostTagInfo(PostTagType.Guild, GuildViewModel.Id, GuildViewModel.Name, null);// GuildViewModel.AvatarLinkWithFallBack);
        return PostSearchHelper.ComputeState(new[] { guildTag.TagString }, _sortModeString, _currentPageString, null, null);
    }
}