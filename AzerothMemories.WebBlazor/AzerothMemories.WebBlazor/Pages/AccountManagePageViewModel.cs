﻿using Microsoft.AspNetCore.Components.Forms;

namespace AzerothMemories.WebBlazor.Pages;

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

    public string CustomAvatarLink { get; private set; }

    public List<string> SocialLogins { get; } = new();// { "Patreon" };

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        AccountViewModel = await Services.ComputeServices.AccountServices.TryGetActiveAccount(Session.Default);

        if (AccountViewModel == null)
        {
        }
        else
        {
            if (NewUsername == null)
            {
                NewUsername = AccountViewModel.Username;

                NewUsernameTextBoxAdornmentColor = Color.Success;
                NewUsernameTextBoxAdornmentIcon = Icons.Material.Filled.CheckCircleOutline;
            }

            if (SocialLinks == null)
            {
                SocialLinks = AccountViewModel.SocialLinks;

                SocialLinksAdornmentIcons = new string[SocialLinks.Length];
                SocialLinksAdornmentColors = new Color[SocialLinks.Length];
            }

            AvatarLink ??= AccountViewModel.Avatar;

            if (CustomAvatarLink == null && AccountViewModel.IsCustomAvatar())
            {
                CustomAvatarLink = AccountViewModel.Avatar;
            }
        }
    }

    public Task OnNewUsernameTextChanged(string username)
    {
        NewUsername = username;

        return CheckValidUsername(NewUsername);
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
            isValid = await Services.ComputeServices.AccountServices.CheckIsValidUsername(Session.Default, username);
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
            NewUsernameTextBoxAdornmentIcon = Icons.Material.Filled.Check;
        }
        else
        {
            NewUsernameTextBoxAdornmentColor = Color.Error;
            NewUsernameTextBoxAdornmentIcon = Icons.Material.Filled.Warning;
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

        var result = await Services.ClientServices.CommandRunner.Run(new Account_TryChangeUsername(Session.Default, 0, NewUsername));
        if (result.Value)
        {
            AccountViewModel.Username = NewUsername;

            NewUsernameTextBoxAdornmentColor = Color.Success;
            NewUsernameTextBoxAdornmentIcon = Icons.Material.Filled.CheckCircleOutline;
        }
        else
        {
            NewUsernameTextBoxAdornmentColor = Color.Error;
            NewUsernameTextBoxAdornmentIcon = Icons.Material.Filled.Warning;
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

        var result = await Services.ClientServices.CommandRunner.Run(new Account_TryChangeIsPrivate(Session.Default, newValue));
        if (AccountViewModel.IsPrivate == result.Value)
        {
            return;
        }

        AccountViewModel.IsPrivate = result.Value;

        OnViewModelChanged?.Invoke();
    }

    public async Task OnBattleTagVisibilityChanged(bool newValue)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.ClientServices.CommandRunner.Run(new Account_TryChangeBattleTagVisibility(Session.Default, newValue));
        if (AccountViewModel.BattleTagIsPublic == result.Value)
        {
            return;
        }

        AccountViewModel.BattleTagIsPublic = result.Value;

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

        await OnChangeAvatarButtonClicked(character.AvatarLink);
    }

    public async Task OnChangeAvatarButtonClicked(string avatarLink)
    {
        var result = await Services.ClientServices.CommandRunner.Run(new Account_TryChangeAvatar(Session.Default, 0, avatarLink));
        if (result.Value != AccountViewModel.Avatar)
        {
            AvatarLink = result.Value;
            AccountViewModel.Avatar = result.Value;
            OnViewModelChanged?.Invoke();
        }
    }

    public async Task UploadCustomAvatar(InputFileChangeEventArgs arg)
    {
        var file = arg.File;
        var extension = Path.GetExtension(file.Name);
        if (!ZExtensions.ValidUploadExtensions.Contains(extension))
        {
            await Services.ClientServices.DialogService.ShowNotificationDialog(false, $"{extension} is not supported yet.");
            return;
        }

        byte[] buffer;
        try
        {
            await using var memoryStream = new MemoryStream();
            var stream = file.OpenReadStream(ZExtensions.MaxAddMemoryFileSizeInBytes);
            await stream.CopyToAsync(memoryStream);
            buffer = memoryStream.ToArray();
        }
        catch (Exception)
        {
            await Services.ClientServices.DialogService.ShowNotificationDialog(false, $"{file.Name} read failed.");
            return;
        }

        var result = await Services.ComputeServices.AccountServices.TryChangeAvatarUpload(new Account_TryChangeAvatarUpload
        {
            Session = Session.Default,
            ImageData = buffer,
        });

        if (result != null && !string.IsNullOrWhiteSpace(result) && result != AccountViewModel.Avatar)
        {
            AvatarLink = result;
            CustomAvatarLink = result;
            AccountViewModel.Avatar = result;
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
            icon = Icons.Material.Filled.Check;
        }
        else if (string.IsNullOrWhiteSpace(newValue))
        {
            if (string.IsNullOrWhiteSpace(oldValue))
            {
            }
            else
            {
                color = Color.Success;
                icon = Icons.Material.Filled.Check;
                shouldChange = true;
            }
        }
        else if (link.ValidatorFunc(newValue))
        {
            shouldChange = true;

            color = Color.Success;
            icon = Icons.Material.Filled.Check;
        }
        else
        {
            color = Color.Error;
            icon = Icons.Material.Filled.Warning;
        }

        SocialLinks[link.LinkId] = newValue;
        SocialLinksAdornmentIcons[link.LinkId] = icon;
        SocialLinksAdornmentColors[link.LinkId] = color;

        if (shouldChange)
        {
            var result = await Services.ClientServices.CommandRunner.Run(new Account_TryChangeSocialLink(Session.Default, 0, link.LinkId, newValue));
            AccountViewModel.SocialLinks[link.LinkId] = result.Value;
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
            var result = await Services.ClientServices.CommandRunner.Run(new Character_TryChangeCharacterAccountSync(Session.Default, character.Id, newValue));
            if (character.AccountSync == result.Value)
            {
                return;
            }

            character.AccountSync = result.Value;

            OnViewModelChanged?.Invoke();
        }
    }

    public async Task OnCharacterDeletedClicked(CharacterViewModel character)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.ClientServices.CommandRunner.Run(new Character_TrySetCharacterDeleted(Session.Default, character.Id));
    }

    public async Task OnCharacterRenamedOrTransferred(CharacterViewModel oldCharacter, CharacterViewModel newCharacter)
    {
        if (AccountViewModel == null)
        {
            return;
        }

        var result = await Services.ClientServices.CommandRunner.Run(new Character_TrySetCharacterRenamedOrTransferred(Session.Default, oldCharacter.Id, newCharacter.Id));
    }

    public async Task OnConnect(ClientAuthHelper clientAuthHelper, string schema)
    {
        await clientAuthHelper.SignIn(schema);
    }

    public async Task OnDisconnect(ClientAuthHelper clientAuthHelper, string schema, string key)
    {
        await Services.ClientServices.CommandRunner.Run(new Account_TryDisconnectAccount(Session.Default, schema, key));
    }
}