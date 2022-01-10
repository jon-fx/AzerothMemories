namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountManagePageViewModel : ViewModelBase
{
    public AccountManagePageViewModel()
    {
        AllAvatars = new Dictionary<long, (string Link, string Name, long Id)>();
    }

    public string NewUsername { get; set; }

    public bool NewUsernameValid { get; private set; }

    public bool ChangeUsernameButtonVisible { get; private set; }

    public Color NewUsernameTextBoxAdornmentColor { get; private set; }

    public string NewUsernameTextBoxAdornmentIcon { get; private set; }

    public ActiveAccountViewModel AccountViewModel { get; private set; }

    public string[] SocialLinks { get; set; }

    public string[] SocialLinksAdornmentIcons { get; private set; }

    public Color[] SocialLinksAdornmentColors { get; private set; }

    public (string Link, string Name, long Id) Avatar { get; set; }

    public Dictionary<long, (string Link, string Name, long Id)> AllAvatars { get; init; }

    public override async Task ComputeState()
    {
        AccountViewModel = await Services.AccountServices.TryGetAccount(null);

        if (AccountViewModel == null)
        {
        }
        else
        {
            NewUsername ??= AccountViewModel.Username;

            NewUsernameTextBoxAdornmentColor = Color.Success;
            NewUsernameTextBoxAdornmentIcon = Icons.Filled.Check;

            SocialLinks ??= AccountViewModel.SocialLinks;

            SocialLinksAdornmentIcons = new string[SocialLinks.Length];
            SocialLinksAdornmentColors = new Color[SocialLinks.Length];

            ResetAvatars();
        }
    }

    private void ResetAvatars()
    {
        var noneKey = 0;
        var currentKey = -1;
        var isDefault = Avatar == default;

        if (!AllAvatars.ContainsKey(noneKey))
        {
            AllAvatars.Add(noneKey, (null, "None", noneKey));

            if (isDefault) Avatar = AllAvatars[noneKey];
        }

        if (!string.IsNullOrWhiteSpace(AccountViewModel.Avatar))
        {
            AllAvatars[currentKey] = (AccountViewModel.Avatar, "Current", currentKey);

            if (isDefault) Avatar = AllAvatars[currentKey];
        }

        foreach (var character in AccountViewModel.GetCharactersSafe())
        {
            AllAvatars[character.Id] = (character.AvatarLinkWithFallBack, character.Name, character.Id);
        }

        if (!isDefault && AllAvatars.TryGetValue(currentKey, out var current) && AllAvatars.TryGetValue(Avatar.Id, out var selected))
        {
            if (current.Link == selected.Link)
            {
                Avatar = current;
            }
        }

        OnViewModelChanged?.Invoke();
    }

    public Task OnNewUsernameTextChanged(string username)
    {
        return CheckValidUsername(username);
    }

    public async Task<bool> CheckValidUsername(string username)
    {
        if (AccountViewModel == null)
        {
            return false;
        }

        var isValid = false;
        var isVisible = false;

        if (string.IsNullOrWhiteSpace(username))
        {
        }
        else if (username == AccountViewModel.Username)
        {
            isValid = true;
        }
        else if (DatabaseHelpers.IsValidAccountName(username))
        {
            isValid = await Services.AccountServices.TryReserveUsername(null, username);
            isVisible = isValid;
        }

        if (NewUsernameValid == isValid && ChangeUsernameButtonVisible == isVisible)
        {
            return NewUsernameValid;
        }

        NewUsernameValid = isValid;
        ChangeUsernameButtonVisible = isVisible;

        if (NewUsernameValid)
        {
            NewUsernameTextBoxAdornmentColor = Color.Success;
            NewUsernameTextBoxAdornmentIcon = Icons.Filled.Check;
        }
        else
        {
            NewUsernameTextBoxAdornmentColor = Color.Error;
            NewUsernameTextBoxAdornmentIcon = Icons.Filled.Warning;
        }

        OnViewModelChanged?.Invoke();

        return NewUsernameValid;
    }

    public async Task OnChangeUsernameClicked()
    {
        if (AccountViewModel == null)
        {
            return;
        }

        if (!AccountViewModel.CanChangeUsername)
        {
            return;
        }

        if (NewUsername == AccountViewModel.Username)
        {
            return;
        }

        ChangeUsernameButtonVisible = false;

        if (!await CheckValidUsername(NewUsername))
        {
            return;
        }

        var result = await Services.AccountServices.TryChangeUsername(null, NewUsername);
        if (result)
        {
            AccountViewModel.Username = NewUsername;
        }

        ChangeUsernameButtonVisible = false;

        OnViewModelChanged?.Invoke();
    }

    public async Task OnIsPrivateChanged(bool newValue)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.AccountServices.TryChangeIsPrivate(null, newValue);
        if (AccountViewModel.IsPrivate == result)
        {
            return;
        }

        AccountViewModel.IsPrivate = newValue;

        OnViewModelChanged?.Invoke();
    }

    public async Task OnBattleTagVisibilityChanged(bool newValue)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.AccountServices.TryChangeBattleTagVisibility(null, newValue);
        if (AccountViewModel.BattleTagIsPublic == result)
        {
            return;
        }

        AccountViewModel.BattleTagIsPublic = newValue;

        OnViewModelChanged?.Invoke();
    }

    public async Task OnChangeAvatarClicked()
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var avatarLink = Avatar.Link;
        if (AccountViewModel.Avatar == avatarLink)
        {
            return;
        }

        AccountViewModel.Avatar = await Services.AccountServices.TryChangeAvatar(null, avatarLink);
        //OnViewModelChanged?.Invoke();
    }

    public async Task OnSocialLinkChanged(SocialHelpers link, string newValue)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var oldValue = AccountViewModel.SocialLinks[link.LinkId];
        var shouldChange = false;
        var color = Color.Error;
        var icon = string.Empty;

        if (newValue == oldValue)
        {
            color = Color.Success;
            icon = Icons.Filled.Check;
        }
        else if (string.IsNullOrWhiteSpace(newValue))
        {
            if (string.IsNullOrWhiteSpace(oldValue))
            {
            }
            else
            {
                color = Color.Success;
                icon = Icons.Filled.Check;
                shouldChange = true;
            }
        }
        else if (link.ValidatorFunc(newValue))
        {
            shouldChange = true;

            color = Color.Success;
            icon = Icons.Filled.Check;
        }
        else
        {
            color = Color.Error;
            icon = Icons.Filled.Warning;
        }

        SocialLinks[link.LinkId] = newValue;
        SocialLinksAdornmentIcons[link.LinkId] = icon;
        SocialLinksAdornmentColors[link.LinkId] = color;

        if (shouldChange)
        {
            AccountViewModel.SocialLinks[link.LinkId] = await Services.AccountServices.TryChangeSocialLink(null, link.LinkId, newValue);
            SocialLinksAdornmentIcons[link.LinkId] = string.Empty;
        }

        OnViewModelChanged?.Invoke();
    }

    public async Task OnAccountSyncToggleChanged(CharacterViewModel character, bool newValue)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        if (character.AccountSync != newValue)
        {
            var result = await Services.CharacterServices.TryChangeCharacterAccountSync(null, character.Id, newValue);
            if (character.AccountSync == result)
            {
                return;
            }

            character.AccountSync = newValue;

            OnViewModelChanged?.Invoke();
        }
    }
}