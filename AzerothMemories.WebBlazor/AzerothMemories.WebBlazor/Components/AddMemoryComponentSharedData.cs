namespace AzerothMemories.WebBlazor.Components;

public sealed class AddMemoryComponentSharedData
{
    private readonly bool _isAddMemoryPage;
    private readonly ViewModelBase _viewModel;

    private PostTagInfo[] _achievementTags;
    private PostTagInfo? _selectedTypeTag;
    private PostTagInfo? _selectedRegionTag;
    private HashSet<PostTagInfo> _selectedCommonTags;
    private HashSet<PostTagInfo> _selectedAchievementTags;
    private HashSet<PostTagInfo> _selectedExtraTags;

    private CharacterViewModel? _selectedCharacter;
    private Func<AccountViewModel?>? _accountViewModelProvider;
    private PostViewModel[] _myPostsAroundPostTimeStamp = [];

    public AddMemoryComponentSharedData(ViewModelBase viewModel, bool isAddMemoryPage)
    {
        _viewModel = viewModel;
        _isAddMemoryPage = isAddMemoryPage;

        TypeTags = _viewModel.Services.ClientServices.TagHelpers.TypeTags;
        RegionTags = _viewModel.Services.ClientServices.TagHelpers.RegionTags;
        CommonTags = _viewModel.Services.ClientServices.TagHelpers.CommonTags;

        SelectedPostAvatarImage = 0;
        PostAvatarImages = [];

        _achievementTags = [];
        _selectedTypeTag = TypeTags[0];
        _selectedRegionTag = RegionTags[0];

        _selectedCommonTags = new HashSet<PostTagInfo>(PostTagInfo.EqualityComparer1);
        _selectedAchievementTags = new HashSet<PostTagInfo>(PostTagInfo.EqualityComparer1);
        _selectedExtraTags = new HashSet<PostTagInfo>(PostTagInfo.EqualityComparer1);
    }

    public bool PrivatePost { get; set; }

    public Instant PostTimeStamp { get; private set; }

    public PostTagInfo[] TypeTags { get; }

    public PostTagInfo[] RegionTags { get; }

    public PostTagInfo[] CommonTags { get; }

    public PostTagInfo[] AchievementTags => _achievementTags;

    public PostTagInfo? SelectedTypeTag => _selectedTypeTag;

    public PostTagInfo? SelectedRegionTag => _selectedRegionTag;

    public IReadOnlyCollection<PostTagInfo> SelectedCommonTags => _selectedCommonTags;

    public IReadOnlyCollection<PostTagInfo> SelectedAchievementTags => _selectedAchievementTags;

    public HashSet<PostTagInfo> SelectedExtraTags => _selectedExtraTags;

    public int SelectedCharacterId => _selectedCharacter?.Id ?? -1;

    public int SelectedPostAvatarImage { get; set; }

    public List<(PostTagInfo? Tag, string ImageLink, string ImageText, string ToolTipText)> PostAvatarImages { get; }

    public Action? OnTagsChanged { get; set; }

    public AccountViewModel? TryGetAccountViewModel => _accountViewModelProvider?.Invoke();

    public Task InitializeAccount(Func<AccountViewModel?> accountViewModelFunc)
    {
        Exceptions.ThrowIf(accountViewModelFunc == null);

        _accountViewModelProvider = accountViewModelFunc;

        var accountViewModel = _accountViewModelProvider();
        if (accountViewModel != null)
        {
            _selectedExtraTags = new HashSet<PostTagInfo>(PostTagInfo.EqualityComparer1);

            if (!string.IsNullOrWhiteSpace(accountViewModel.Avatar))
            {
                PostAvatarImages.Add((null, accountViewModel.Avatar, accountViewModel.GetAvatarText(), "Default"));
            }

            _selectedExtraTags.Add(new PostTagInfo(PostTagType.Account, accountViewModel.Id, accountViewModel.GetDisplayName(), null) { IsChipClosable = false });
        }

        OnTagsChanged?.Invoke();

        return Task.CompletedTask;
    }

    public async Task InitializeAchievements()
    {
        var timeStamp = PostTimeStamp.ToUnixTimeMilliseconds();
        if (timeStamp > 0 && PostTimeStamp < SystemClock.Instance.GetCurrentInstant())
        {
            var achievements = await _viewModel.Services.ComputeServices.AccountServices.TryGetAchievementsByTime(_viewModel.Services.ClientServices.Session, timeStamp, 120, ServerSideLocaleExt.GetServerSideLocale());

            if (_selectedAchievementTags.Count > 0)
            {
                SelectedAchievementTagsChanged(new List<PostTagInfo>());
            }

            _achievementTags = achievements;
            _viewModel.OnViewModelChanged();
        }
    }

    public async Task SetPostTimeStamp(Instant postTimeStamp)
    {
        if (PostTimeStamp == postTimeStamp)
        {
            return;
        }

        PostTimeStamp = postTimeStamp;

        await InitializeAchievements();

        if (_isAddMemoryPage)
        {
            var timeStamp = PostTimeStamp.ToUnixTimeMilliseconds();
            var myPostsAroundPostTimeStamp = await _viewModel.Services.ComputeServices.AccountServices.TrySearchPostsByTime(_viewModel.Services.ClientServices.Session, timeStamp, 120, ServerSideLocaleExt.GetServerSideLocale());
            if (myPostsAroundPostTimeStamp.SequenceEqual(_myPostsAroundPostTimeStamp))
            {
            }
            else
            {
                _myPostsAroundPostTimeStamp = myPostsAroundPostTimeStamp;
                _viewModel.OnViewModelChanged();
            }
        }
    }

    public void OnEditingPost(PostViewModel currentPost)
    {
        foreach (var tagInfo in currentPost.SystemTags.SafeEnumerable())
        {
            var mainTag = TypeTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, tagInfo));
            if (mainTag != null)
            {
                _selectedTypeTag = mainTag;
                continue;
            }

            var regionTag = RegionTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, tagInfo));
            if (regionTag != null)
            {
                _selectedRegionTag = regionTag;
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

            //var selectedExtraTags = _selectedExtraTags.FirstOrDefault(x => PostTagInfo.EqualityComparer1.Equals(x, tagInfo));
            //if (selectedExtraTags != null)
            //{
            //    continue;
            //}

            if (tagInfo.Type == PostTagType.Character)
            {
                var accountViewModel = _accountViewModelProvider?.Invoke();
                var character = accountViewModel.GetCharactersForTagSafe(_selectedTypeTag).FirstOrDefault(x => x.Id == tagInfo.Id);
                if (character != null)
                {
                    ChangeSelectedCharacter(character.Id);
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

        _viewModel.OnViewModelChanged();

        OnTagsChanged?.Invoke();
    }

    public async Task<AddMemoryResult> Submit(PublishCommentComponent commentComponent, List<AddMemoryImageData> uploadResults)
    {
        var timeStamp = PostTimeStamp;
        var finalText = commentComponent.GetCommentText();
        var systemTags = GetSystemHashTags();

        string? avatarTag = null;
        if (SelectedPostAvatarImage > 0 && SelectedPostAvatarImage < PostAvatarImages.Count)
        {
            avatarTag = PostAvatarImages[SelectedPostAvatarImage].Tag?.TagString;
        }

        var imageData = new List<byte[]?>();
        foreach (var uploadResult in uploadResults)
        {
            if (uploadResult.EditedFileContent != null)
            {
                uploadResult.FileContent = uploadResult.EditedFileContent;
                uploadResult.EditedFileContent = null;
            }

            if (uploadResult.FileContent != null)
            {
                imageData.Add(uploadResult.FileContent);
            }
        }

        var serverUploadResult = await _viewModel.Services.ClientServices.CommandRunner.Run(new Post_TryPostMemory
        {
            Session = _viewModel.Services.ClientServices.Session,
            TimeStamp = timeStamp.ToUnixTimeMilliseconds(),
            AvatarTag = avatarTag ?? string.Empty,
            IsPrivate = PrivatePost,
            Comment = finalText,
            SystemTags = [.. systemTags],
            ImageData = imageData
        });

        return serverUploadResult.Value;
    }

    public async Task<AddMemoryResultCode> SubmitOnEditingPost(PostViewModel currentPost)
    {
        var newTags = GetSystemHashTags();

        string? avatarTag = null;
        if (SelectedPostAvatarImage > 0 && SelectedPostAvatarImage < PostAvatarImages.Count)
        {
            avatarTag = PostAvatarImages[SelectedPostAvatarImage].Tag?.TagString;
        }

        var result = await _viewModel.Services.ClientServices.CommandRunner.Run(new Post_TryUpdateSystemTags(_viewModel.Services.ClientServices.Session, currentPost.Id, avatarTag, newTags));
        return result.Value;
    }

    private HashSet<string> GetSystemHashTags()
    {
        var isRetailSelected = _selectedTypeTag == TypeTags[0];
        if (!isRetailSelected)
        {
            _selectedExtraTags.RemoveWhere(x => x.Type.IsRetailOnlyTag());
        }

        var allTags = new List<PostTagInfo>();
        if (_selectedTypeTag != null)
        {
            allTags.Add(_selectedTypeTag);
        }

        if (_selectedRegionTag != null)
        {
            allTags.Add(_selectedRegionTag);
        }

        foreach (var tagInfo in _selectedCommonTags)
        {
            allTags.Add(tagInfo);
        }

        foreach (var tagInfo in _selectedAchievementTags)
        {
            allTags.Add(tagInfo);
        }

        foreach (var tagInfo in _selectedExtraTags)
        {
            allTags.Add(tagInfo);
        }

        var tagsAsTags = new HashSet<string>();
        foreach (var tag in allTags)
        {
            tagsAsTags.Add(tag.TagString);
        }

        return tagsAsTags;
    }

    public void SelectedMainTagsChanged(PostTagInfo postTagInfo)
    {
        _selectedTypeTag = postTagInfo;

        var isRetailSelected = _selectedTypeTag == TypeTags[0];
        if (!isRetailSelected)
        {
            TryRemoveSelectedCharacterInfo();

            _selectedExtraTags.RemoveWhere(x => x.Type.IsRetailOnlyTag());
        }

        OnTagsChanged?.Invoke();
    }

    public void SelectedRegionTagsChanged(PostTagInfo postTagInfo)
    {
        _selectedRegionTag = postTagInfo;

        OnTagsChanged?.Invoke();
    }

    public void SelectedCommonTagsChanged(IReadOnlyCollection<PostTagInfo> collection)
    {
        _selectedCommonTags = collection.ToHashSet(PostTagInfo.EqualityComparer1);

        OnTagsChanged?.Invoke();
    }

    public void SelectedAchievementTagsChanged(IReadOnlyCollection<PostTagInfo> collection)
    {
        var addedSet = new HashSet<PostTagInfo>(collection);
        addedSet.ExceptWith(_selectedAchievementTags);

        var removedSet = new HashSet<PostTagInfo>(_selectedAchievementTags);
        removedSet.ExceptWith(collection);

        foreach (var tagInfo in addedSet)
        {
            AddImageToSelection(tagInfo);
        }

        foreach (var tagInfo in removedSet)
        {
            RemoveImageFromSelection(tagInfo);
        }

        _selectedAchievementTags = collection.ToHashSet(PostTagInfo.EqualityComparer1);

        OnTagsChanged?.Invoke();
    }

    public void ChangeSelectedCharacter(int newSelectedCharacter)
    {
        if (SelectedCharacterId == newSelectedCharacter)
        {
            return;
        }

        TryRemoveSelectedCharacterInfo();

        var accountViewModel = _accountViewModelProvider?.Invoke();

        _selectedCharacter = accountViewModel.GetCharactersForTagSafe(_selectedTypeTag).FirstOrDefault(x => x.Id == newSelectedCharacter);
        if (_selectedCharacter != null)
        {
            //_selectedTypeTag = TypeTags[0];

            var stringLocalizer = _viewModel.Services.ClientServices.BlizzardStringLocalizer;
            var characterName = $"{_selectedCharacter.Name} ({stringLocalizer[$"Realm-{_selectedCharacter.RealmId}"]})";
            var characterNameTag = new PostTagInfo(PostTagType.Character, _selectedCharacter.Id, characterName, _selectedCharacter.GetAvatarLinkWithFallBack());
            var characterRealmTag = new PostTagInfo(PostTagType.Realm, _selectedCharacter.RealmId, stringLocalizer[$"Realm-{_selectedCharacter.RealmId}"], null);

            var characterRegionId = _selectedCharacter.RegionId.ToValue();
            var characterRegionTag = RegionTags.FirstOrDefault(x => x.Id == characterRegionId);
            if (characterRegionTag != null)
            {
                _selectedRegionTag = characterRegionTag;
            }
            else
            {
                _selectedRegionTag = RegionTags[0];
            }

            _selectedExtraTags.Add(characterNameTag);
            _selectedExtraTags.Add(characterRealmTag);

            AddImageToSelection(characterNameTag);
            AddImageToSelection(characterRealmTag);
        }

        OnTagsChanged?.Invoke();
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

            var characterRealmTag = _selectedExtraTags.FirstOrDefault(x => x.Type == PostTagType.Realm && x.Id == _selectedCharacter.RealmId);
            if (characterRealmTag != null)
            {
                _selectedExtraTags.Remove(characterRealmTag);

                RemoveImageFromSelection(characterRealmTag);
            }

            _selectedCharacter = null;
        }
    }

    private void AddImageToSelection(PostTagInfo? postTag)
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

    private void RemoveImageFromSelection(PostTagInfo? postTag)
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

    public void OnSelectedMainTagChipClose(MudChip<PostTagInfo> mudChip)
    {
        var postTagInfo = mudChip.Value;
        if (postTagInfo == null)
        {
            return;
        }

        _selectedTypeTag = postTagInfo;

        RemoveImageFromSelection(postTagInfo);

        OnTagsChanged?.Invoke();
    }

    public void OnSelectedCommonTagChipClose(MudChip<PostTagInfo> mudChip)
    {
        var postTagInfo = mudChip.Value;
        if (postTagInfo == null)
        {
            return;
        }

        _selectedCommonTags.Remove(postTagInfo);

        RemoveImageFromSelection(postTagInfo);

        OnTagsChanged?.Invoke();
    }

    public void OnSelectedExtraTagChipClose(MudChip<PostTagInfo> mudChip)
    {
        var postTagInfo = mudChip.Value;
        if (postTagInfo == null)
        {
            return;
        }

        _selectedExtraTags.Remove(postTagInfo);

        RemoveImageFromSelection(postTagInfo);

        if (_selectedCharacter != null)
        {
            if (postTagInfo.Type == PostTagType.Character && _selectedCharacter.Id == postTagInfo.Id)
            {
                TryRemoveSelectedCharacterInfo();
            }
            //else if (postTagInfo.Type == PostTagType.Realm && _selectedCharacter.RealmId == postTagInfo.Id)
            //{
            //    TryRemoveSelectedCharacterInfo();
            //}
        }

        OnTagsChanged?.Invoke();
    }

    public void OnSelectedAchievementTagChipClose(MudChip<PostTagInfo> mudChip)
    {
        var postTagInfo = mudChip.Value;
        if (postTagInfo == null)
        {
            return;
        }

        _selectedAchievementTags.Remove(postTagInfo);

        RemoveImageFromSelection(postTagInfo);

        OnTagsChanged?.Invoke();
    }

    public (string[] ErrorMessages, PostViewModel[] PostViewModels) GetErrorStrings()
    {
        var errorStrings = new List<string>();
        var allTagCounters = new int[ZExtensions.TagCountsPerPost.Length];

        var timeNow = SystemClock.Instance.GetCurrentInstant();
        if (PostTimeStamp <= ZExtensions.MinPostTime || PostTimeStamp >= timeNow)
        {
            var minTime = _viewModel.Services.ClientServices.TimeProvider.GetTimeAsLocalString(ZExtensions.MinPostTime);
            var maxTime = _viewModel.Services.ClientServices.TimeProvider.GetTimeAsLocalString(timeNow);

            errorStrings.Add($"Time must be between {minTime} and {maxTime}.");
        }

        var selectedRegionTag = SelectedRegionTag;
        if (selectedRegionTag != null && selectedRegionTag.Id > 0)
        {
        }
        else
        {
            errorStrings.Add("A region must be selected.");
        }

        var allTags = new List<PostTagInfo>();
        if (_selectedTypeTag != null)
        {
            var tagInfo = _selectedTypeTag;
            allTagCounters[(int)tagInfo.Type]++;
            allTags.Add(tagInfo);
        }

        if (_selectedRegionTag != null)
        {
            var tagInfo = _selectedRegionTag;
            allTagCounters[(int)tagInfo.Type]++;
            allTags.Add(tagInfo);
        }

        foreach (var tagInfo in SelectedCommonTags)
        {
            allTagCounters[(int)tagInfo.Type]++;
            allTags.Add(tagInfo);
        }

        foreach (var tagInfo in SelectedExtraTags)
        {
            allTagCounters[(int)tagInfo.Type]++;
            allTags.Add(tagInfo);
        }

        foreach (var tagInfo in SelectedAchievementTags)
        {
            allTagCounters[(int)tagInfo.Type]++;
            allTags.Add(tagInfo);
        }

        for (var i = 0; i < allTagCounters.Length; i++)
        {
            var count = allTagCounters[i];
            var minMax = ZExtensions.TagCountsPerPost[i];
            if (count >= minMax.Min && count <= minMax.Max)
            {
            }
            else
            {
                errorStrings.Add($"{(PostTagType)i} tag count must be between {minMax.Min} and {minMax.Max}.");
            }
        }

        foreach (var tagInfo in allTags)
        {
            if (tagInfo.MinTagTime > 0)
            {
                var minTagTime = Instant.FromUnixTimeMilliseconds(tagInfo.MinTagTime);
                if (minTagTime > PostTimeStamp)
                {
                    var minTagTimeStr = _viewModel.Services.ClientServices.TimeProvider.GetTimeAsLocalString(minTagTime);
                    errorStrings.Add($"{tagInfo.Name} can not be used in posts before {minTagTimeStr}.");
                }
            }
        }

        var myPostsAroundPostTimeStamp = _myPostsAroundPostTimeStamp;

        return (errorStrings.ToArray(), myPostsAroundPostTimeStamp);
    }
}