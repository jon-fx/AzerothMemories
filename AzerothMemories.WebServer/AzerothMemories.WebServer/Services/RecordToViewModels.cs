namespace AzerothMemories.WebServer.Services;

public static class RecordToViewModels
{
    public static void PopulateViewModel(AccountViewModel accountViewModel, AccountRecord accountRecord, Dictionary<long, AccountFollowingViewModel> followingViewModels, Dictionary<long, AccountFollowingViewModel> followersViewModels)
    {
        accountViewModel.Id = accountRecord.Id;
        accountViewModel.Avatar = accountRecord.Avatar;
        accountViewModel.Username = accountRecord.Username;
        accountViewModel.AccountType = accountRecord.AccountType;
        accountViewModel.RegionId = accountRecord.BlizzardRegionId;
        accountViewModel.BattleTag = accountRecord.BattleTag;
        accountViewModel.BattleTagIsPublic = accountRecord.BattleTagIsPublic;
        accountViewModel.CreatedDateTime = accountRecord.CreatedDateTime.ToUnixTimeMilliseconds();
        accountViewModel.IsPrivate = accountRecord.IsPrivate;

        accountViewModel.SocialLinks = new[]
        {
            accountRecord.SocialDiscord,
            accountRecord.SocialTwitter,
            accountRecord.SocialTwitch,
            accountRecord.SocialYouTube,
        };

        accountViewModel.FollowingViewModels = RemoveNoneStatus(followingViewModels);
        accountViewModel.FollowersViewModels = RemoveNoneStatus(followersViewModels);
    }

    private static Dictionary<long, AccountFollowingViewModel> RemoveNoneStatus(Dictionary<long, AccountFollowingViewModel> viewModels)
    {
        var results = new Dictionary<long, AccountFollowingViewModel>();
        foreach (var kvp in viewModels)
        {
            if (kvp.Value.Status == AccountFollowingStatus.None)
            {
                continue;
            }

            results.Add(kvp.Key, kvp.Value);
        }

        return results;
    }

    public static AccountViewModel CreateAccountViewModel(this AccountRecord accountRecord, Dictionary<long, CharacterViewModel> characters, Dictionary<long, AccountFollowingViewModel> followingViewModels, Dictionary<long, AccountFollowingViewModel> followersViewModels)
    {
        var viewModel = new ActiveAccountViewModel();

        PopulateViewModel(viewModel, accountRecord, followingViewModels, followersViewModels);

        if (viewModel.BattleTagIsPublic)
        {
        }
        else
        {
            viewModel.BattleTag = null;
        }

        viewModel.CharactersArray = characters.Values.Where(x => x.AccountSync).ToArray();

        return viewModel;
    }

    public static ActiveAccountViewModel CreateActiveAccountViewModel(this AccountRecord accountRecord, Dictionary<long, CharacterViewModel> characters, Dictionary<long, AccountFollowingViewModel> followingViewModels, Dictionary<long, AccountFollowingViewModel> followersViewModels)
    {
        var viewModel = new ActiveAccountViewModel();

        PopulateViewModel(viewModel, accountRecord, followingViewModels, followersViewModels);

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
            GuildId = characterRecord.BlizzardGuildId,
            GuildName = characterRecord.BlizzardGuildName,
            LastUpdateHttpResult = characterRecord.UpdateJobLastResult
        };
    }

    public static PostViewModel CreatePostViewModel(PostRecord postRecord, AccountViewModel accountViewModel, bool canSeePost, PostReactionViewModel reactionRecord, PostTagInfo[] postTagRecords)
    {
        var viewModel = new PostViewModel
        {
            Id = postRecord.Id,
            AccountId = postRecord.AccountId,
            AccountUsername = accountViewModel.Username,
            AccountAvatar = accountViewModel.Avatar,
            PostComment = postRecord.PostComment,
            PostVisibility = postRecord.PostVisibility,
            PostTime = postRecord.PostTime.ToUnixTimeMilliseconds(),
            PostCreatedTime = postRecord.PostCreatedTime.ToUnixTimeMilliseconds(),
            PostEditedTime = postRecord.PostEditedTime.ToUnixTimeMilliseconds(),
            ImageBlobNames = postRecord.BlobNames.Split('|'),
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
            DeletedTimeStamp = postRecord.DeletedTimeStamp,
            SystemTags = postTagRecords,
        };

        if (postRecord.PostAvatar != null)
        {
            viewModel.PostAvatar = viewModel.SystemTags.First(x => x.TagString == postRecord.PostAvatar).Image;
        }

        if (!canSeePost)
        {
            viewModel.PostComment = null;
            viewModel.PostVisibility = 255;
            viewModel.ImageBlobNames = Array.Empty<string>();
            viewModel.TotalReactionCount = 0;
            viewModel.TotalCommentCount = 0;

            Array.Fill(viewModel.ReactionCounters, 0);
        }

        return viewModel;
    }

    public static PostCommentViewModel CreateCommentViewModel(PostCommentRecord comment, string username, string avatar)
    {
        var viewModel = new PostCommentViewModel
        {
            Id = comment.Id,
            AccountId = comment.AccountId,
            PostId = comment.PostId,
            ParentId = comment.ParentId.GetValueOrDefault(),
            AccountUsername = username,
            AccountAvatar = avatar,
            PostComment = comment.PostComment,
            CreatedTime = comment.CreatedTime.ToUnixTimeMilliseconds(),
            DeletedTimeStamp = comment.DeletedTimeStamp,
            //ReactionId = reaction?.Id ?? 0,
            //Reaction = reaction?.Reaction ?? PostReaction.None,
            TotalReactionCount = comment.TotalReactionCount,
            ReactionCounters = new[]
            {
                comment.ReactionCount1,
                comment.ReactionCount2,
                comment.ReactionCount3,
                comment.ReactionCount4,
                comment.ReactionCount5,
                comment.ReactionCount6,
                comment.ReactionCount7,
                comment.ReactionCount8,
                comment.ReactionCount9
            }
        };

        return viewModel;
    }

    public static PostCommentReactionViewModel CreatePostCommentReactionViewModel(PostCommentReactionRecord reactionRecord, string username)
    {
        var viewModel = new PostCommentReactionViewModel
        {
            Id = reactionRecord.Id,
            CommentId = reactionRecord.CommentId,
            AccountId = reactionRecord.AccountId,
            AccountUsername = username,
            Reaction = reactionRecord.Reaction,
            LastUpdateTime = reactionRecord.LastUpdateTime.ToUnixTimeMilliseconds(),
        };

        return viewModel;
    }
}