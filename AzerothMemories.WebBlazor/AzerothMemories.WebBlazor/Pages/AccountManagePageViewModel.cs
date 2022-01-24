﻿namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountManagePageViewModel : ViewModelBase
{
    public string NewUsername { get; set; }

    public bool NewUsernameValid { get; private set; }

    public bool ChangeUsernameButtonVisible { get; private set; }

    public Color NewUsernameTextBoxAdornmentColor { get; private set; }

    public string NewUsernameTextBoxAdornmentIcon { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public string[] SocialLinks { get; set; }

    public string[] SocialLinksAdornmentIcons { get; private set; }

    public Color[] SocialLinksAdornmentColors { get; private set; }

    public string AvatarLink { get; private set; }

    public override async Task ComputeState()
    {
        AccountViewModel = await Services.ComputeServices.AccountServices.TryGetActiveAccount(null);

        if (AccountViewModel == null)
        {
        }
        else
        {
            if (NewUsername == null)
            {
                NewUsername = AccountViewModel.Username;

                NewUsernameTextBoxAdornmentColor = Color.Success;
                NewUsernameTextBoxAdornmentIcon = Icons.Filled.CheckCircleOutline;
            }

            if (SocialLinks == null)
            {
                SocialLinks = AccountViewModel.SocialLinks;

                SocialLinksAdornmentIcons = new string[SocialLinks.Length];
                SocialLinksAdornmentColors = new Color[SocialLinks.Length];
            }

            AvatarLink ??= AccountViewModel.Avatar;
        }
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
            isValid = await Services.ComputeServices.AccountServices.CheckIsValidUsername(null, username);
            isVisible = true;
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

        var result = await Services.CommandRunner.Run(new Account_TryChangeUsername { Session = Services.Session, NewUsername = NewUsername });
        if (result.Result)
        {
            AccountViewModel.Username = NewUsername;

            NewUsernameTextBoxAdornmentColor = Color.Success;
            NewUsernameTextBoxAdornmentIcon = Icons.Filled.CheckCircleOutline;
        }
        else
        {
            NewUsernameTextBoxAdornmentColor = Color.Error;
            NewUsernameTextBoxAdornmentIcon = Icons.Filled.Warning;
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

        var result = await Services.CommandRunner.Run(new Account_TryChangeIsPrivate { Session = Services.Session, NewValue = newValue });
        if (AccountViewModel.IsPrivate == result.Result)
        {
            return;
        }

        AccountViewModel.IsPrivate = result.Result;

        OnViewModelChanged?.Invoke();
    }

    public async Task OnBattleTagVisibilityChanged(bool newValue)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.CommandRunner.Run(new Account_TryChangeBattleTagVisibility { Session = Services.Session, NewValue = newValue });
        if (AccountViewModel.BattleTagIsPublic == result.Result)
        {
            return;
        }

        AccountViewModel.BattleTagIsPublic = result.Result;

        OnViewModelChanged?.Invoke();
    }

    public async Task OnChangeAvatarButtonClicked(CharacterViewModel character)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        if (character == null)
        {
            return;
        }

        if (AvatarLink == character.AvatarLink)
        {
            return;
        }

        var result = await Services.CommandRunner.Run(new Account_TryChangeAvatar { Session = Services.Session, NewAvatar = character.AvatarLink });
        if (result.Result != AccountViewModel.Avatar)
        {
            AvatarLink = result.Result;
            AccountViewModel.Avatar = result.Result;
            OnViewModelChanged?.Invoke();
        }
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
            var result = await Services.CommandRunner.Run(new Account_TryChangeSocialLink { Session = Services.Session, LinkId = link.LinkId, NewValue = newValue });
            AccountViewModel.SocialLinks[link.LinkId] = result.Result;
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
            var result = await Services.CommandRunner.Run(new Character_TryChangeCharacterAccountSync(Services.Session, character.Id, newValue));
            if (character.AccountSync == result.Result)
            {
                return;
            }

            character.AccountSync = result.Result;

            OnViewModelChanged?.Invoke();
        }
    }

    public async Task OnCharacterDeletedClicked(CharacterViewModel character)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.CommandRunner.Run(new Character_TrySetCharacterDeleted(Services.Session, character.Id));
    }

    public async Task OnCharacterRenamedOrTransferred(CharacterViewModel oldCharacter, CharacterViewModel newCharacter)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.CommandRunner.Run(new Character_TrySetCharacterRenamedOrTransferred(Services.Session, oldCharacter.Id, newCharacter.Id));
    }
}