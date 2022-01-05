using AzerothMemories.WebBlazor.Common;

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

    public DateTimeOffset PostTimeStamp { get; private set; }

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

    public Task InitializeAccount(ActiveAccountViewModel accountViewModel)
    {
        Exceptions.ThrowIf(accountViewModel == null);

        //_accountViewModel = accountViewModel;
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
        var timeStamp = PostTimeStamp.ToUniversalTime().ToUnixTimeMilliseconds();
        if (timeStamp > 0 && DateTimeOffset.FromUnixTimeMilliseconds(timeStamp) < DateTimeOffset.UtcNow)
        {
            var achievements = await _viewModel.Services.AccountServices.TryGetAchievementsByTime(null, timeStamp, 120);

            if (_selectedAchievementTags.Count > 0)
            {
                SelectedAchievementTagsChanged(new List<object>());
            }

            _achievementTags = achievements;
            _viewModel.OnViewModelChanged?.Invoke();
        }
    }

    public async Task SetPostTimeStamp(DateTimeOffset postTimeStamp)
    {
        PostTimeStamp = postTimeStamp;

        await InitializeAchievements();
    }

    //public async Task OnEditingPost(PostViewModel currentPost)
    //{
    //    if (currentPost.SystemTagsArray == null)
    //    {
    //        await currentPost.UpdateSystemTags(currentPost.SystemTags, _services);
    //    }

    //    foreach (var tagInfo in currentPost.SystemTagsArray)
    //    {
    //        var mainTag = MainTags.FirstOrDefault(x => TagHelpers.AreEqual(x, tagInfo));
    //        if (mainTag != null)
    //        {
    //            _selectedMainTags.Add(mainTag);

    //            continue;
    //        }

    //        var commonTag = CommonTags.FirstOrDefault(x => TagHelpers.AreEqual(x, tagInfo));
    //        if (commonTag != null)
    //        {
    //            _selectedCommonTags.Add(commonTag);

    //            continue;
    //        }

    //        var achievementTag = _achievementTags.FirstOrDefault(x => TagHelpers.AreEqual(x, tagInfo));
    //        if (achievementTag != null)
    //        {
    //            if (_selectedAchievementTags.Add(achievementTag))
    //            {
    //                AddImageToSelection(achievementTag);
    //            }

    //            continue;
    //        }

    //        var selectedExtraTags = _selectedExtraTags.FirstOrDefault(x => TagHelpers.AreEqual(x, tagInfo));
    //        if (selectedExtraTags != null)
    //        {
    //            continue;
    //        }

    //        if (tagInfo.Type == PostTagType.Character && _accountViewModel.CharactersDict.TryGetValue(tagInfo.Id, out var character))
    //        {
    //            await ChangeSelectedCharacter(character.Id);
    //        }

    //        if (_selectedExtraTags.Add(tagInfo))
    //        {
    //            AddImageToSelection(tagInfo);
    //        }
    //    }

    //    if (!string.IsNullOrWhiteSpace(currentPost.PostAvatar))
    //    {
    //        for (var i = 0; i < PostAvatarImages.Count; i++)
    //        {
    //            var postAvatarImage = PostAvatarImages[i];
    //            if (postAvatarImage.ImageLink == currentPost.PostAvatar)
    //            {
    //                SelectedPostAvatarImage = i;
    //                break;
    //            }
    //        }
    //    }

    //    OnModelChanged?.Invoke();
    //}

    public async Task<AddMemoryComponentResult> Submit(PublishCommentComponent commentComponent, List<AddMemoryUploadResult> uploadResults)
    {
        var timeStamp = PostTimeStamp;
        var finalText = commentComponent.GetCommentText();
        var systemTags = GetSystemHashTags();

        string avatarImage = null;
        string avatarTag = null;
        if (SelectedPostAvatarImage > 0 && SelectedPostAvatarImage < PostAvatarImages.Count)
        {
            avatarImage = PostAvatarImages[SelectedPostAvatarImage].ImageLink;
            avatarTag = PostAvatarImages[SelectedPostAvatarImage].Tag.TagString;
        }

        var transferData = new AddMemoryTransferData(timeStamp.ToUnixTimeMilliseconds(), avatarImage, avatarTag, PrivatePost, finalText, systemTags, uploadResults);
        var (result, postId) = await _viewModel.Services.PostServices.TryPostMemory(null, transferData);

        return new AddMemoryComponentResult(transferData, result, _viewModel.Services.ActiveAccountServices.ActiveAccountId, postId);
    }

    //public async Task<(bool Result, string SystemTags)> SubmitOnEditingPost(PostViewModel currentPost)
    //{
    //    var newTags = GetSystemHashTags();
    //    var currentTags = currentPost.SystemTagsArray.Select(x => x.TagString).ToHashSet();

    //    var addedSet = new HashSet<string>(newTags);
    //    addedSet.ExceptWith(currentTags);

    //    var removedSet = new HashSet<string>(currentTags);
    //    removedSet.ExceptWith(newTags);

    //    if (addedSet.Count == 0 && removedSet.Count == 0)
    //    {
    //        return (false, null);
    //    }

    //    var avatarImage = currentPost.PostAvatar;
    //    if (SelectedPostAvatarImage > 0 && SelectedPostAvatarImage < PostAvatarImages.Count)
    //    {
    //        avatarImage = PostAvatarImages[SelectedPostAvatarImage].ImageLink;
    //    }

    //    var postProvider = _services.CommonServices.ClusterClient.GetGrain<IPostGrain>(currentPost.Id);
    //    var result = await postProvider.OnEditMemory(avatarImage, currentPost.SystemTags, addedSet, removedSet);

    //    await currentPost.UpdateSystemTags(result.SystemTags, _services);

    //    return result;
    //}

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

        _selectedCharacter = _viewModel.Services.ActiveAccountServices.AccountViewModel.CharactersArray.FirstOrDefault(x => x.Id == newSelectedCharacter);
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

    //public async Task AddSearchDataToTags(AccountSearchResult searchResult)
    //{
    //    var postTag = await TagHelpers.CreatePostTag(_services, searchResult);

    //    AddSearchDataToTags(postTag);
    //}

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