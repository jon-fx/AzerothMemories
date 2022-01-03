using AzerothMemories.WebBlazor.ViewModels;

namespace AzerothMemories.WebServer.Services
{
    public static class RecordToViewModels
    {
        public static void PopulateViewModel(AccountViewModel accountViewModel, AccountRecord accountRecord)
        {
            accountViewModel.Id = accountRecord.Id;
            accountViewModel.Avatar = accountRecord.Avatar;
            accountViewModel.Username = accountRecord.Username;
            accountViewModel.RegionId = accountRecord.BlizzardRegionId;
            accountViewModel.BattleTag = accountRecord.BattleTag;
            accountViewModel.BattleTagIsPublic = accountRecord.BattleTagIsPublic;
            accountViewModel.CreatedDateTime = accountRecord.CreatedDateTime;
            accountViewModel.IsPrivate = accountRecord.IsPrivate;
        }

        public static AccountViewModel CreateAccountViewModel(this AccountRecord accountRecord, Dictionary<long, CharacterViewModel> characters)
        {
            var viewModel = new ActiveAccountViewModel();

            PopulateViewModel(viewModel, accountRecord);

            viewModel.CharactersArray = characters.Values.Where(x => x.AccountSync).ToArray();

            return viewModel;
        }

        public static ActiveAccountViewModel CreateActiveAccountViewModel(this AccountRecord accountRecord, Dictionary<long, CharacterViewModel> characters)
        {
            var viewModel = new ActiveAccountViewModel();

            PopulateViewModel(viewModel, accountRecord);

            viewModel.CharactersArray = characters.Values.ToArray();

            return viewModel;
        }

        public static CharacterViewModel CreateViewModel(this CharacterRecord characterRecord)
        {
            return new CharacterViewModel
            {
                Id = characterRecord.Id,
                Ref = characterRecord.MoaRef,
                Race = characterRecord.Race,
                Class = characterRecord.Class,
                Level = characterRecord.Level,
                Gender = characterRecord.Gender,
                AvatarLink = characterRecord.AvatarLink,
                AccountSync = characterRecord.AccountSync,
                Name = characterRecord.Name,
                RealmId = characterRecord.RealmId,
                RegionId = characterRecord.BlizzardRegionId,
                GuildId = characterRecord.GuildId,
                GuildName = characterRecord.GuildName,
                LastUpdateHttpResult = characterRecord.UpdateJobLastResult
            };
        }
    }
}