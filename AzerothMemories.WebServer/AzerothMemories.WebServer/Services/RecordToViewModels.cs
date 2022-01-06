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
            accountViewModel.CreatedDateTime = accountRecord.CreatedDateTime.ToUnixTimeMilliseconds();
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

        public static PostViewModel CreatePostViewModel(PostRecord postRecord, AccountViewModel accountViewModel, PostReactionRecord reactionRecord)
        {
            return new()
            {
                Id = postRecord.Id,
                AccountId = postRecord.AccountId,
                AccountUsername = accountViewModel.Username,
                AccountAvatar = accountViewModel.Avatar,
                PostComment = postRecord.PostComment,
                PostAvatar = postRecord.PostAvatar,
                PostVisibility = postRecord.PostVisibility,
                PostTime = postRecord.PostTime.ToUnixTimeMilliseconds(),
                PostCreatedTime = postRecord.PostCreatedTime.ToUnixTimeMilliseconds(),
                PostEditedTime = postRecord.PostEditedTime.ToUnixTimeMilliseconds(),
                ImageBlobNames = postRecord.BlobNames.Split('|'),
                SystemTags = postRecord.SystemTags,
                ReactionId = reactionRecord?.Id ?? 0,
                Reaction = reactionRecord?.Reaction ?? 0,
                ReactionCounters = new[]
                {
                    postRecord.ReactionCount1,
                    postRecord.ReactionCount2,
                    postRecord.ReactionCount3,
                    postRecord.ReactionCount4,
                    postRecord.ReactionCount5,
                    postRecord.ReactionCount6,
                    postRecord.ReactionCount7,
                    postRecord.ReactionCount8,
                    postRecord.ReactionCount9
                },
                TotalReactionCount = postRecord.TotalReactionCount,
                TotalCommentCount = postRecord.TotalCommentCount,
                DeletedTimeStamp = postRecord.DeletedTimeStamp
            };
        }
    }
}