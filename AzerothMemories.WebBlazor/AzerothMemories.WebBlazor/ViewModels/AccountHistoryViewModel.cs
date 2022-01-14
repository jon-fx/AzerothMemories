namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountHistoryViewModel
{
    [JsonInclude] public long Id;
    [JsonInclude] public AccountHistoryType Type;

    [JsonInclude] public long AccountId;
    [JsonInclude] public long OtherAccountId;
    [JsonInclude] public string OtherAccountUsername;

    [JsonInclude] public long TargetId;
    [JsonInclude] public long TargetPostId;
    [JsonInclude] public long TargetCommentId;

    [JsonInclude] public long CreatedTime;

    public string GetDisplayText(ActiveAccountViewModel activeAccountViewModel, IStringLocalizer<BlizzardResources> stringLocalizer)
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

                return $"<a href='post/{TargetId}/{TargetPostId}/'>Memory restored</a>.";
            }
            case AccountHistoryType.MemoryDeleted:
            {
                return "Memory deleted.";
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

                    //name = $"<a class='wowclass-{character.Class}' href='character/{character.Id}'>{character.Name}</a> ({stringLocalizer[$"Realm-{character.RealmId}"]})";
                }

                return $"Character {name} updated.";
            }
            case AccountHistoryType.FollowingRequestSent:
            {
                return $"You sent a following request to <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>.";
            }
            case AccountHistoryType.FollowingRequestReceived:
            {
                return $"You received a following request from <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>.";
            }
            case AccountHistoryType.FollowingRequestAccepted1:
            {
                return $"You accepted a following request from <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>.";
            }
            case AccountHistoryType.FollowingRequestAccepted2:
            {
                return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> accepted your following request.";
            }
            case AccountHistoryType.FollowerRemoved:
            {
                return $"You removed <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> as a follower.";
            }
            case AccountHistoryType.StartedFollowing:
            {
                return $"You started following <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>.";
            }
            case AccountHistoryType.StoppedFollowing:
            {
                return $"You stopped following <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>.";
            }
            case AccountHistoryType.Commented1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                return $"You commented on <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>'s <a href='post/{TargetId}/{TargetPostId}/?comment={TargetCommentId}'>post</a>.";
            }
            case AccountHistoryType.Commented2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> comment on your <a href='post/{TargetId}/{TargetPostId}?comment={TargetCommentId}'>post</a>.";
            }
            case AccountHistoryType.ReactedToPost1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"You reacted to <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>'s <a href='post/{TargetId}/{TargetPostId}'>post</a>.";
            }
            case AccountHistoryType.ReactedToPost2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> reacted to your <a href='post/{TargetId}/{TargetPostId}'>post</a>.";
            }
            case AccountHistoryType.ReactedToComment1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                return $"You reacted to <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>'s <a href='post/{TargetId}/{TargetPostId}/?comment={TargetCommentId}'>comment</a>.";
            }
            case AccountHistoryType.ReactedToComment2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> reacted to your <a href='post/{TargetId}/{TargetPostId}/?comment={TargetCommentId}'>comment</a>.";
            }
            case AccountHistoryType.TaggedPost:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> tagged you in a <a href='post/{TargetId}/{TargetPostId}'>post</a>.";
            }
            case AccountHistoryType.TaggedComment:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId == 0);

                return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> tagged you in a <a href='post/{TargetId}/{TargetPostId}/?comment={TargetCommentId}'>comment</a>.";
            }
            case AccountHistoryType.MemoryRestoredExternal1:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"You tagged yourself in <a href='account/{OtherAccountId}'>{OtherAccountUsername}</a>'s <a href='post/{TargetId}/{TargetPostId}/'>memory</a>.";
            }
            case AccountHistoryType.MemoryRestoredExternal2:
            {
                Exceptions.ThrowIf(TargetId == 0);
                Exceptions.ThrowIf(TargetPostId == 0);
                Exceptions.ThrowIf(TargetCommentId != 0);

                return $"<a href='account/{OtherAccountId}'>{OtherAccountUsername}</a> tagged themself in your <a href='post/{TargetId}/{TargetPostId}/'>memory</a>.";
            }
            case AccountHistoryType.None:
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}