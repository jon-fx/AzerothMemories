﻿namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class CharacterPagePageViewModel : ViewModelBase
    {
        public string ErrorMessage { get; private set; }

        public AccountViewModel AccountViewModel { get; private set; }

        public CharacterViewModel CharacterViewModel { get; private set; }

        public PostSearchHelper PostSearchHelper { get; private set; }

        public bool IsLoading => CharacterViewModel == null || PostSearchHelper == null;

        public override Task OnInitialized()
        {
            PostSearchHelper = new PostSearchHelper(Services);

            return Task.CompletedTask;
        }

        public async Task ComputeState(long id, string region, string realm, string name, string sortModeString, string currentPageString)
        {
            AccountViewModel accountViewModel;
            CharacterViewModel characterViewModel;
            if (id > 0)
            {
                var result = await Services.CharacterServices.TryGetCharacter(null, id);
                accountViewModel = result.AccountViewModel;
                characterViewModel = result.CharacterViewModel;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(region))
                {
                    ErrorMessage = "Invalid Region";
                    return;
                }

                if (string.IsNullOrWhiteSpace(realm))
                {
                    ErrorMessage = "Invalid Realm";
                    return;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    ErrorMessage = "Invalid Name";
                    return;
                }

                var regionInfo = BlizzardRegionInfo.AllByName.Values.FirstOrDefault(x => x.TwoLetters.ToLowerInvariant() == region);
                if (regionInfo == null)
                {
                    ErrorMessage = "Invalid Region";
                    return;
                }

                if (!Services.TagHelpers.GetRealmId(realm, out _))
                {
                    ErrorMessage = "Invalid Realm";
                    return;
                }

                var result = await Services.CharacterServices.TryGetCharacter(null, regionInfo.Region, realm, name);

                accountViewModel = result.AccountViewModel;
                characterViewModel = result.CharacterViewModel;
            }

            if (characterViewModel == null)
            {
                ErrorMessage = "Invalid Character";
                return;
            }

            AccountViewModel = accountViewModel;
            CharacterViewModel = characterViewModel;

            var accountTag = new PostTagInfo(PostTagType.Character, CharacterViewModel.Id, CharacterViewModel.Name, CharacterViewModel.AvatarLinkWithFallBack);
            await PostSearchHelper.ComputeState(new[] { accountTag.TagString }, sortModeString, currentPageString, null, null);
        }
    }
}