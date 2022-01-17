namespace AzerothMemories.WebBlazor.Components;

public sealed class AddMemoryComponentSharedData
{
    private readonly ViewModelBase _viewModel;

    private PostTagInfo[] _achievementTags;
    private HashSet<object> _selectedMainTags;
    private HashSet<object> _selectedCommonTags;
    private HashSet<object> _selectedAchievementTags;
    private HashSet<PostTagInfo> _selectedExtraTags;

    //private AccountViewModel _accountViewModel;
    private CharacterViewModel _selectedCharacter;

    private Func<AccountViewModel> _accountViewModelProvider;

    public AddMemoryComponentSharedData(ViewModelBase viewModel)
    {
        _viewModel = viewModel;

        MainTags = _viewModel.Services.TagHelpers.MainTags;
        CommonTags = _viewModel.Services.TagHelpers.CommonTags;

        SelectedPostAvatarImage = 0;
        PostAvatarImages = new List<(PostTagInfo Tag, string ImageLink, string ImageText, string ToolTipText)>();

        _achievementTags = Array.Empty<PostTagInfo>();
        _selectedMainTags = new HashSet<object>(PostTagInfo.EqualityComparer2);
        _selectedCommonTags = new HashSet<object>(PostTagInfo.EqualityComparer2);
        _selectedAchievementTags = new HashSet<object>(PostTagInfo.EqualityComparer2);
        _selectedExtraTags = new HashSet<PostTagInfo>(PostTagInfo.EqualityComparer2);

        //UploadResults = new List<AddMemoryUploadResult>();
        //ErrorStrings = new List<(Severity, string)>();
    }

    public bool PrivatePost { get; set; }

    public Instant PostTimeStamp { get; private set; }

    public PostTagInfo[] MainTags { get; }

    public PostTagInfo[] CommonTags { get; }

    public PostTagInfo[] AchievementTags => _achievementTags;

    public ICollection<object> SelectedMainTags => _selectedMainTags;

    public ICollection<object> SelectedCommonTags => _selectedCommonTags;

    public ICollection<object> SelectedAchievementTags => _selectedAchievementTags;

    public HashSet<PostTagInfo> SelectedExtraTags => _selectedExtraTags;

    public long SelectedCharacterId => _selectedCharacter?.Id ?? -1;

    public int SelectedPostAvatarImage { get; set; }

    public List<(PostTagInfo Tag, string ImageLink, string ImageText, string ToolTipText)> PostAvatarImages { get; }

    public Action OnTagsChanged { get; set; }

    public AccountViewModel TryGetAccountViewModel => _accountViewModelProvider();

    public Task InitializeAccount(Func<AccountViewModel> accountViewModelFunc)
    {
        Exceptions.ThrowIf(accountViewModelFunc == null);

        _accountViewModelProvider = accountViewModelFunc;

        var accountViewModel = _accountViewModelProvider();

        _selectedExtraTags = new HashSet<PostTagInfo>(PostTagInfo.EqualityComparer2);

        PostAvatarImages.Add((null, accountViewModel.Avatar, accountViewModel.GetAvatarText(), "Default"));

        var accountRegion = accountViewModel.RegionId.ToInfo();

        _selectedExtraTags.Add(new PostTagInfo(PostTagType.Account, accountViewModel.Id, accountViewModel.GetDisplayName(), null) { IsChipClosable = false });
        _selectedExtraTags.Add(new PostTagInfo(PostTagType.Region, (int)accountRegion.Region, accountRegion.Name, null) { IsChipClosable = false });

        OnTagsChanged?.Invoke();

        return Task.CompletedTask;
    }

    public async Task InitializeAchievements()
    {
        var timeStamp = PostTimeStamp.ToUnixTimeMilliseconds();
        if (timeStamp > 0 && PostTimeStamp < SystemClock.Instance.GetCurrentInstant())
        {
            var achievements = await _viewModel.Services.AccountServices.TryGetAchievementsByTime(null, timeStamp, 120, CultureInfo.CurrentCulture.Name);

            if (_selectedAchievementTags.Count > 0)
            {
                SelectedAchievementTagsChanged(new List<object>());
            }

            _achievementTags = achievements;
            _viewModel.OnViewModelChanged?.Invoke();
        }
    }

    public async Task SetPostTimeStamp(Instant postTimeStamp)
    {
        PostTimeStamp = postTimeStamp;

        await InitializeAchievements();
    }

    public async Task OnEditingPost(PostViewModel currentPost)
    {
        foreach (var tagInfo in currentPost.SystemTags)
        {
            var mainTag = MainTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, tagInfo));
            if (mainTag != null)
            {
                _selectedMainTags.Add(mainTag);

                continue;
            }

            var commonTag = CommonTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, tagInfo));
            if (commonTag != null)
            {
                _selectedCommonTags.Add(commonTag);

                continue;
            }

            var achievementTag = _achievementTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, tagInfo));
            if (achievementTag != null)
            {
                if (_selectedAchievementTags.Add(achievementTag))
                {
                    AddImageToSelection(achievementTag);
                }

                continue;
            }

            var selectedExtraTags = _selectedExtraTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, tagInfo));
            if (selectedExtraTags != null)
            {
                continue;
            }

            var accountViewModel = _accountViewModelProvider();
            if (tagInfo.Type == PostTagType.Character)
            {
                var character = accountViewModel.GetCharactersSafe().FirstOrDefault(x => x.Id == tagInfo.Id);
                if (character != null)
                {
                    await ChangeSelectedCharacter(character.Id);
                }
            }

            if (_selectedExtraTags.Add(tagInfo))
            {
                AddImageToSelection(tagInfo);
            }
        }

        if (!string.IsNullOrWhiteSpace(currentPost.PostAvatar))
        {
            for (var i = 0; i < PostAvatarImages.Count; i++)
            {
                var postAvatarImage = PostAvatarImages[i];
                if (postAvatarImage.ImageLink == currentPost.PostAvatar)
                {
                    SelectedPostAvatarImage = i;
                    break;
                }
            }
        }

        _viewModel.OnViewModelChanged?.Invoke();
    }

    public async Task<AddMemoryResult> Submit(PublishCommentComponent commentComponent, List<AddMemoryUploadResult> uploadResults)
    {
        var timeStamp = PostTimeStamp;
        var finalText = commentComponent.GetCommentText();
        var systemTags = GetSystemHashTags();

        //string avatarImage = null;
        string avatarTag = null;
        if (SelectedPostAvatarImage > 0 && SelectedPostAvatarImage < PostAvatarImages.Count)
        {
            //avatarImage = PostAvatarImages[SelectedPostAvatarImage].ImageLink;
            avatarTag = PostAvatarImages[SelectedPostAvatarImage].Tag.TagString;
        }

        var transferData = new AddMemoryTransferData(timeStamp.ToUnixTimeMilliseconds(), avatarTag, PrivatePost, finalText, systemTags, uploadResults);
        var result = await _viewModel.Services.PostServices.TryPostMemory(null, transferData);

        return result;
    }

    public async Task<AddMemoryResultCode> SubmitOnEditingPost(PostViewModel currentPost)
    {
        var newTags = GetSystemHashTags();

        string avatarTag = null;
        if (SelectedPostAvatarImage > 0 && SelectedPostAvatarImage < PostAvatarImages.Count)
        {
            avatarTag = PostAvatarImages[SelectedPostAvatarImage].Tag.TagString;
        }

        var result = await _viewModel.Services.PostServices.TryUpdateSystemTags(null, currentPost.Id, new TryUpdateSystemTagsInfo
        {
            AvatarText = avatarTag,
            NewTags = newTags
        });

        return result;
    }

    private HashSet<string> GetSystemHashTags()
    {
        var isRetailSelected = _selectedMainTags.FirstOrDefault() == MainTags[0];
        if (!isRetailSelected)
        {
            _selectedExtraTags.RemoveWhere(x => x.Type.IsRetailOnlyTag());
        }

        var allTags = new List<PostTagInfo>();
        foreach (var mudChip in _selectedMainTags)
        {
            var tag = (PostTagInfo)mudChip;
            allTags.Add(tag);
        }

        foreach (var mudChip in _selectedCommonTags)
        {
            var tag = (PostTagInfo)mudChip;
            allTags.Add(tag);
        }

        foreach (var mudChip in _selectedAchievementTags)
        {
            var tag = (PostTagInfo)mudChip;
            allTags.Add(tag);
        }

        foreach (var tag in _selectedExtraTags)
        {
            allTags.Add(tag);
        }

        var tagsAsTags = new HashSet<string>();
        foreach (var tag in allTags)
        {
            tagsAsTags.Add(tag.TagString);
        }

        return tagsAsTags;
    }

    public void SelectedMainTagsChanged(ICollection<object> collection)
    {
        _selectedMainTags = collection.ToHashSet();

        var isRetailSelected = _selectedMainTags.FirstOrDefault() == MainTags[0];
        if (!isRetailSelected)
        {
            TryRemoveSelectedCharacterInfo();

            _selectedExtraTags.RemoveWhere(x => x.Type.IsRetailOnlyTag());
        }

        OnTagsChanged?.Invoke();
    }

    public void SelectedCommonTagsChanged(ICollection<object> collection)
    {
        _selectedCommonTags = collection.ToHashSet();

        OnTagsChanged?.Invoke();
    }

    public void SelectedAchievementTagsChanged(ICollection<object> collection)
    {
        var addedSet = new HashSet<object>(collection);
        addedSet.ExceptWith(_selectedAchievementTags);

        var removedSet = new HashSet<object>(_selectedAchievementTags);
        removedSet.ExceptWith(collection);

        foreach (var obj in addedSet)
        {
            AddImageToSelection((PostTagInfo)obj);
        }

        foreach (var obj in removedSet)
        {
            RemoveImageFromSelection((PostTagInfo)obj);
        }

        _selectedAchievementTags = collection.ToHashSet();
        //_viewModel.OnViewModelChanged?.Invoke();

        OnTagsChanged?.Invoke();
    }

    public Task ChangeSelectedCharacter(long newSelectedCharacter)
    {
        if (SelectedCharacterId == newSelectedCharacter)
        {
            return Task.CompletedTask;
        }

        TryRemoveSelectedCharacterInfo();

        var accountViewModel = _accountViewModelProvider();

        _selectedCharacter = accountViewModel.GetCharactersSafe().FirstOrDefault(x => x.Id == newSelectedCharacter);
        if (_selectedCharacter != null)
        {
            var stringLocalizer = _viewModel.Services.StringLocalizer;
            var characterName = $"{_selectedCharacter.Name} ({stringLocalizer[$"Realm-{_selectedCharacter.RealmId}"]})";
            var characterNameTag = new PostTagInfo(PostTagType.Character, _selectedCharacter.Id, characterName, _selectedCharacter.AvatarLinkWithFallBack);
            var characterRealmTag = new PostTagInfo(PostTagType.Realm, _selectedCharacter.RealmId, stringLocalizer[$"Realm-{_selectedCharacter.RealmId}"], null);

            AddImageToSelection(characterNameTag);

            _selectedExtraTags.Add(characterNameTag);
            _selectedExtraTags.Add(characterRealmTag);

            if (_selectedCharacter.GuildId > 0)
            {
                //SelectedExtraTags.Add(await TagHelpers.CreatePostTag(_services, PostTagType.Guild, _selectedCharacter.GuildId));
            }
        }

        OnTagsChanged?.Invoke();
        //_viewModel.OnViewModelChanged?.Invoke();

        return Task.CompletedTask;
    }

    private void TryRemoveSelectedCharacterInfo()
    {
        if (_selectedCharacter != null)
        {
            var characterNameTag = _selectedExtraTags.FirstOrDefault(x => x.Type == PostTagType.Character && x.Id == _selectedCharacter.Id);
            if (characterNameTag != null)
            {
                _selectedExtraTags.Remove(characterNameTag);

                RemoveImageFromSelection(characterNameTag);
            }

            _selectedExtraTags.RemoveWhere(x => x.Type == PostTagType.Realm && x.Id == _selectedCharacter.RealmId);
            _selectedExtraTags.RemoveWhere(x => x.Type == PostTagType.Guild && x.Id == _selectedCharacter.GuildId);
        }
    }

    private void AddImageToSelection(PostTagInfo postTag)
    {
        if (postTag == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(postTag.Image))
        {
            return;
        }

        var first = PostAvatarImages.FirstOrDefault(x => x.Tag == postTag);
        if (first == default)
        {
            PostAvatarImages.Add((postTag, postTag.Image, "?", postTag.NameWithIcon));
        }
    }

    private void RemoveImageFromSelection(PostTagInfo postTag)
    {
        if (postTag == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(postTag.Image))
        {
            return;
        }

        var first = PostAvatarImages.FirstOrDefault(x => x.Tag == postTag);
        if (first == default)
        {
        }
        else
        {
            PostAvatarImages.Remove(first);
        }

        if (SelectedPostAvatarImage > PostAvatarImages.Count)
        {
            SelectedPostAvatarImage = 0;
        }
    }

    public void AddSearchDataToTags(PostTagInfo postTag)
    {
        var extra = _selectedExtraTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, postTag));
        var achievement = _achievementTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, postTag));

        if (extra != null)
        {
        }
        else if (_selectedExtraTags.Count > 64)
        {
        }
        else if (achievement != null)
        {
            if (_selectedAchievementTags.Add(achievement))
            {
                AddImageToSelection(achievement);

                OnTagsChanged?.Invoke();
            }
        }
        else
        {
            if (_selectedExtraTags.Add(postTag))
            {
                AddImageToSelection(postTag);

                OnTagsChanged?.Invoke();
            }
        }
    }

    public void AddSearchDataToTags(MainSearchResult searchResult)
    {
        AddSearchDataToTags(searchResult.ToTagInfo());
    }

    public void OnSelectedMainTagChipClose(MudChip mudChip)
    {
        var postTagInfo = (PostTagInfo)mudChip.Value;
        _selectedMainTags.Remove(postTagInfo);

        RemoveImageFromSelection(postTagInfo);

        OnTagsChanged?.Invoke();
    }

    public void OnSelectedCommonTagChipClose(MudChip mudChip)
    {
        var postTagInfo = (PostTagInfo)mudChip.Value;
        _selectedCommonTags.Remove(postTagInfo);

        RemoveImageFromSelection(postTagInfo);

        OnTagsChanged?.Invoke();
    }

    public void OnSelectedExtraTagChipClose(MudChip mudChip)
    {
        var postTagInfo = (PostTagInfo)mudChip.Value;
        _selectedExtraTags.Remove(postTagInfo);

        RemoveImageFromSelection(postTagInfo);

        OnTagsChanged?.Invoke();
    }

    public void OnSelectedAchievementTagChipClose(MudChip mudChip)
    {
        var postTagInfo = (PostTagInfo)mudChip.Value;
        _selectedAchievementTags.Remove(postTagInfo);

        RemoveImageFromSelection(postTagInfo);

        OnTagsChanged?.Invoke();
    }
}