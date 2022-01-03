namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class AccountManagePageViewModel : ViewModelBase
    {
        public AccountManagePageViewModel()
        {
            AllAvatars = new List<(string, string)>();
        }

        public override async Task ComputeState(CancellationToken cancellationToken)
        {
            AccountViewModel = await Services.AccountServices.TryGetAccount(null, cancellationToken);

            if (AccountViewModel == null)
            {
            }
            else
            {
                NewUsername = AccountViewModel.Username;
                NewUsernameTextBoxAdornmentColor = Color.Success;
                NewUsernameTextBoxAdornmentIcon = Icons.Filled.Check;

                //SocialLinks = AccountViewModel.SocialLinks;
                //SocialLinksAdornmentIcons = new string[SocialLinks.Length];
                //SocialLinksAdornmentColors = new Color[SocialLinks.Length];

                var none = ("", "None");
                if (AccountViewModel.Avatar == null)
                {
                    Avatar = none;
                    AllAvatars = new List<(string, string)> { none };
                }
                else
                {
                    Avatar = (AccountViewModel.Avatar, "Current");
                    AllAvatars = new List<(string, string)> { none, Avatar };
                }

                foreach (var character in AccountViewModel.CharactersArray)
                {
                    AllAvatars.Add((character.AvatarLinkWithFallBack, character.Name));
                }
            }
        }

        public string NewUsername { get; set; }

        public bool NewUsernameValid { get; private set; }

        public bool ChangeUsernameButtonVisible { get; private set; }

        public Color NewUsernameTextBoxAdornmentColor { get; private set; }

        public string NewUsernameTextBoxAdornmentIcon { get; private set; }

        public ActiveAccountViewModel AccountViewModel { get; private set; }

        public (string, string) Avatar { get; set; }

        public List<(string, string)> AllAvatars { get; private set; }

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
            throw new NotImplementedException();
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
}