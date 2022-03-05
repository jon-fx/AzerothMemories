namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountHistoryViewModel
{
    [JsonInclude] public int Id;
    [JsonInclude] public AccountHistoryType Type;

    [JsonInclude] public int AccountId;
    [JsonInclude] public int OtherAccountId;
    [JsonInclude] public string OtherAccountUsername;

    [JsonInclude] public int TargetId;
    [JsonInclude] public int TargetPostId;
    [JsonInclude] public int TargetCommentId;

    [JsonInclude] public long CreatedTime;

    public string GetDisplayText(AccountViewModel activeAccountViewModel, IStringLocalizer<BlizzardResources> stringLocalizer)
    {
        switch (Type)
        {
            case AccountHistoryType.AccountCreated:
            {
                return "Account created.";
            }
            case AccountHistoryType.UsernameChanged:
            {
                return "Username changed.";
            }
            case AccountHistoryType.MemoryRestored:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"{GetPostLink("Memory restored")}.";
            }
            case AccountHistoryType.MemoryDeleted:
            {
                if (AccountId == OtherAccountId)
                {
                    return "Memory deleted.";
                }

                return "Memory deleted by admin.";
            }
            case AccountHistoryType.CommentDeleted:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetPostCommentLink("Comment")} deleted.";
                }

                return $"{GetPostCommentLink("Comment")} deleted by admin.";
            }
            case AccountHistoryType.CharacterUpdated:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId != 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                var name = $"<a href='character/{TargetId}'>Unknown</a>";
                var character = activeAccountViewModel.GetCharactersSafe().FirstOrDefault(x => x.Id == TargetId);
                if (character != null && character.Ref != null)
                {
                    var moaRef = new MoaRef(character.Ref);
                    var region = moaRef.Region.ToInfo();
                    var nameLink = $"<a class='wowclass-{character.Class}' href='character/{region.TwoLetters}/{moaRef.Realm}/{character.Name}'>{character.Name}</a>";
                    name = $"{nameLink} ({stringLocalizer[$"Realm-{character.RealmId}"]})";
                }

                return $"Character {name} updated.";
            }
            case AccountHistoryType.FollowingRequestSent:
            {
                return $"{GetYouLink()} sent a following request to {GetOtherAccountLink()}.";
            }
            case AccountHistoryType.FollowingRequestReceived:
            {
                return $"{GetYouLink()} received a following request from {GetOtherAccountLink()}.";
            }
            case AccountHistoryType.FollowingRequestAccepted1:
            {
                return $"{GetYouLink()} accepted a following request from {GetOtherAccountLink()}.";
            }
            case AccountHistoryType.FollowingRequestAccepted2:
            {
                return $"{GetOtherAccountLink()} accepted your following request.";
            }
            case AccountHistoryType.FollowerRemoved:
            {
                return $"{GetYouLink()} removed {GetOtherAccountLink()} as a follower.";
            }
            case AccountHistoryType.StartedFollowing:
            {
                if (TargetId == 0)
                {
                    return $"{GetYouLink()} started following {GetOtherAccountLink()}.";
                }

                return $"{GetOtherAccountLink()} started following {GetYouLink("you")}.";
            }
            case AccountHistoryType.StoppedFollowing:
            {
                return $"{GetYouLink()} stopped following {GetOtherAccountLink()}.";
            }
            case AccountHistoryType.Commented1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} {GetPostCommentLink("commented")} on {GetYouLink("your own")} {GetPostLink("post")}.";
                }

                return $"{GetYouLink()} {GetPostCommentLink("commented")} on {GetOtherAccountLink()}'s {GetPostLink("post")}.";
            }
            case AccountHistoryType.Commented2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} {GetPostCommentLink("commented")} on {GetYouLink("your own")} {GetPostLink("post")}.";
                }

                return $"{GetOtherAccountLink()} {GetPostCommentLink("commented")} on your {GetPostLink("post")}.";
            }
            case AccountHistoryType.ReactedToPost1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} reacted to {GetYouLink("your own")} {GetPostLink("post")}.";
                }

                return $"{GetYouLink()} reacted to {GetOtherAccountLink()}'s {GetPostLink("post")}.";
            }
            case AccountHistoryType.ReactedToPost2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} reacted to {GetYouLink("your own")} {GetPostLink("post")}.";
                }

                return $"{GetOtherAccountLink()} reacted to your {GetPostLink("post")}.";
            }
            case AccountHistoryType.ReactedToComment1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} reacted to {GetYouLink("your own")} {GetPostCommentLink("comment")}.";
                }

                return $"{GetYouLink()} reacted to {GetOtherAccountLink()}'s {GetPostCommentLink("comment")}.";
            }
            case AccountHistoryType.ReactedToComment2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} reacted to {GetYouLink("your own")} {GetPostCommentLink("comment")}.";
                }

                return $"{GetOtherAccountLink()} reacted to your {GetPostCommentLink("comment")}.";
            }
            case AccountHistoryType.TaggedPost:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} tagged yourself in a {GetPostLink("post")}.";
                }

                return $"{GetOtherAccountLink()} tagged {GetYouLink("you")} in a {GetPostLink("post")}.";
            }
            case AccountHistoryType.TaggedComment:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} tagged {GetYouLink("yourself")} in a {GetPostCommentLink("comment")}.";
                }

                return $"{GetOtherAccountLink()} tagged {GetYouLink("you")} in a {GetPostCommentLink("comment")}.";
            }
            case AccountHistoryType.MemoryRestoredExternal1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                if (AccountId == OtherAccountId)
                {
                    return $"{GetYouLink()} tagged yourself in {GetYouLink("your own")} {GetPostLink("memory")}.";
                }

                return $"{GetYouLink()} tagged yourself in {GetOtherAccountLink()}'s {GetPostLink("memory")}.";
            }
            case AccountHistoryType.MemoryRestoredExternal2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"{GetOtherAccountLink()} tagged themself in your {GetPostLink("memory")}.";
            }
            case AccountHistoryType.PostReported:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"{GetYouLink()} reported {GetOtherAccountLink()} {GetPostLink("post")}.";
            }
            case AccountHistoryType.PostReportedComment:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                return $"{GetYouLink()} reported {GetOtherAccountLink()}'s {GetPostCommentLink("comment")}.";
            }
            case AccountHistoryType.PostReportedTags:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"{GetYouLink()} reported tags on {GetOtherAccountLink()}'s {GetPostLink("post")}.";
            }
            case AccountHistoryType.None:
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    private string GetYouLink(string text = "You")
    {
        return $"<a href='account/{AccountId}'>{text}</a>";
    }

    private string GetPostLink(string text)
    {
        Exceptions.ThrowIf(TargetId == 0);
        Exceptions.ThrowIf(TargetPostId == 0);

        return $"<a href='post/{TargetId}/{TargetPostId}'>{text}</a>";
    }

    private string GetPostCommentLink(string text)
    {
        Exceptions.ThrowIf(TargetId == 0);
        Exceptions.ThrowIf(TargetPostId == 0);
        Exceptions.ThrowIf(TargetCommentId == 0);

        return $"<a href='post/{TargetId}/{TargetPostId}?comment={TargetCommentId}'>{text}</a>";
    }

    private string GetOtherAccountLink()
    {
        return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>";
    }
}