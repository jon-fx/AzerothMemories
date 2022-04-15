using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;
using System.Data;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0003_AccountData : Migration
{
    public const int MigrationId = 3;

    public override void Up()
    {
        const string Id = "Id";

        Create.Table(AccountRecord.TableName)
            .WithColumn(nameof(AccountRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountRecord.FusionId)).AsText().Unique().NotNullable()
            .WithColumn(nameof(AccountRecord.AccountType)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.AccountFlags)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BattleTag)).AsText().Nullable()
            .WithColumn(nameof(AccountRecord.BattleTagIsPublic)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(AccountRecord.Username)).AsText().Unique().Nullable()
            .WithColumn(nameof(AccountRecord.UsernameSearchable)).AsText().Nullable().Indexed()
            .WithColumn(nameof(AccountRecord.UsernameChangedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(AccountRecord.IsPrivate)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(AccountRecord.Avatar)).AsText().Nullable()
            .WithColumn(nameof(AccountRecord.SocialDiscord)).AsText().Nullable()
            .WithColumn(nameof(AccountRecord.SocialTwitter)).AsText().Nullable()
            .WithColumn(nameof(AccountRecord.SocialTwitch)).AsText().Nullable()
            .WithColumn(nameof(AccountRecord.SocialYouTube)).AsText().Nullable()
            .WithColumn(nameof(AccountRecord.LastLoginTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(AccountRecord.LoginConsecutiveDaysCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.CreatedDateTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(AccountRecord.BanReason)).AsText().Nullable()
            .WithColumn(nameof(AccountRecord.BanExpireTime)).AsDateTimeOffsetWithDefault();

        Create.Table(AccountFollowingRecord.TableName)
            .WithColumn(nameof(AccountFollowingRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountFollowingRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(AccountFollowingRecord.FollowerId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(AccountFollowingRecord.Status)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountFollowingRecord.LastUpdateTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(AccountFollowingRecord.CreatedTime)).AsDateTimeOffsetWithDefault();
        
        Create.Table(AuthTokenRecord.TableName)
            .WithColumn(nameof(AuthTokenRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AuthTokenRecord.Key)).AsText().Unique().NotNullable()
            .WithColumn(nameof(AuthTokenRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable().Indexed()
            .WithColumn(nameof(AuthTokenRecord.Name)).AsText().Nullable()
            .WithColumn(nameof(AuthTokenRecord.Token)).AsText().Nullable()
            .WithColumn(nameof(AuthTokenRecord.RefreshToken)).AsText().Nullable()
            .WithColumn(nameof(AuthTokenRecord.TokenExpiresAt)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(AuthTokenRecord.LastUpdateTime)).AsDateTimeOffsetWithDefault();

        Create.Table(GuildRecord.TableName)
            .WithColumn(nameof(GuildRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(GuildRecord.MoaRef)).AsText().Unique().NotNullable()
            .WithColumn(nameof(GuildRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.Name)).AsText().Nullable()
            .WithColumn(nameof(GuildRecord.NameSearchable)).AsText().Nullable().Indexed()
            .WithColumn(nameof(GuildRecord.RealmId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.Faction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.MemberCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.AchievementPoints)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.CreatedDateTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(GuildRecord.BlizzardCreatedTimestamp)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(GuildRecord.AchievementTotalPoints)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.AchievementTotalQuantity)).AsInt32().WithDefaultValue(0);

        Create.Table(GuildAchievementRecord.TableName)
            .WithColumn(nameof(GuildAchievementRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(GuildAchievementRecord.GuildId)).AsInt32().ForeignKey(GuildRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(GuildAchievementRecord.AchievementId)).AsInt32().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(GuildAchievementRecord.AchievementTimeStamp)).AsDateTimeOffsetWithDefault().Indexed();

        Create.Table(CharacterRecord.TableName)
            .WithColumn(nameof(CharacterRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterRecord.MoaRef)).AsText().Unique().NotNullable()
            .WithColumn(nameof(CharacterRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardAccountId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Name)).AsText().Nullable()
            .WithColumn(nameof(CharacterRecord.NameSearchable)).AsText().Nullable().Indexed()
            .WithColumn(nameof(CharacterRecord.CreatedDateTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(CharacterRecord.CharacterStatus)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable().Indexed()
            .WithColumn(nameof(CharacterRecord.AccountSync)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(CharacterRecord.RealmId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Class)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Race)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Gender)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Level)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Faction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.AvatarLink)).AsText().Nullable()
            .WithColumn(nameof(CharacterRecord.AchievementTotalPoints)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.AchievementTotalQuantity)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.GuildId)).AsInt32().ForeignKey(GuildRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable().Indexed()
            .WithColumn(nameof(CharacterRecord.GuildRef)).AsText().Nullable()
            .WithColumn(nameof(CharacterRecord.BlizzardGuildRank)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardGuildName)).AsText().Nullable();

        Create.Table(CharacterAchievementRecord.TableName)
            .WithColumn(nameof(CharacterAchievementRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterAchievementRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable().Indexed()
            .WithColumn(nameof(CharacterAchievementRecord.CharacterId)).AsInt32().ForeignKey(CharacterRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(CharacterAchievementRecord.AchievementId)).AsInt32().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(CharacterAchievementRecord.AchievementTimeStamp)).AsDateTimeOffsetWithDefault().Indexed();

        Create.Table(BlizzardUpdateRecord.TableName)
            .WithColumn(nameof(BlizzardUpdateRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(BlizzardUpdateRecord.UpdateStatus)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(BlizzardUpdateRecord.UpdatePriority)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(BlizzardUpdateRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Nullable().Indexed()
            .WithColumn(nameof(BlizzardUpdateRecord.CharacterId)).AsInt32().ForeignKey(CharacterRecord.TableName, Id).OnDelete(Rule.Cascade).Nullable().Indexed()
            .WithColumn(nameof(BlizzardUpdateRecord.GuildId)).AsInt32().ForeignKey(GuildRecord.TableName, Id).OnDelete(Rule.Cascade).Nullable().Indexed()
            .WithColumn(nameof(BlizzardUpdateRecord.UpdateLastModified)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(BlizzardUpdateRecord.UpdateJobLastEndTime)).AsDateTimeOffsetWithDefault();

        Create.Table(BlizzardUpdateChildRecord.TableName)
            .WithColumn(nameof(BlizzardUpdateChildRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(BlizzardUpdateChildRecord.ParentId)).AsInt32().ForeignKey(BlizzardUpdateRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(BlizzardUpdateChildRecord.UpdateType)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(BlizzardUpdateChildRecord.UpdateTypeString)).AsString()
            .WithColumn(nameof(BlizzardUpdateChildRecord.UpdateFailCounter)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(BlizzardUpdateChildRecord.BlizzardLastModified)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(BlizzardUpdateChildRecord.UpdateJobLastResult)).AsInt16().WithDefaultValue(0);

        Create.Table(PostRecord.TableName)
            .WithColumn(nameof(PostRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostRecord.PostCommentRaw)).AsText().Nullable()
            .WithColumn(nameof(PostRecord.PostCommentMark)).AsText().Nullable()
            .WithColumn(nameof(PostRecord.PostCommentUserMap)).AsText().Nullable()
            .WithColumn(nameof(PostRecord.PostAvatar)).AsText().Nullable()
            .WithColumn(nameof(PostRecord.PostVisibility)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.PostFlags)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.BlobNames)).AsText()
            .WithColumn(nameof(PostRecord.PostTime)).AsDateTimeOffsetWithDefault().Indexed()
            .WithColumn(nameof(PostRecord.PostEditedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(PostRecord.PostCreatedTime)).AsDateTimeOffsetWithDefault().Indexed()
            .WithReactionInfo()
            .WithColumn(nameof(PostRecord.TotalCommentCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.TotalViewCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.TotalReportCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.DeletedTimeStamp)).AsInt64().WithDefaultValue(0).Indexed();

        Create.Table(PostCommentRecord.TableName)
            .WithColumn(nameof(PostCommentRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostCommentRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostCommentRecord.ParentId)).AsInt32().ForeignKey(PostCommentRecord.TableName, Id).OnDelete(Rule.Cascade).Nullable().Indexed()
            .WithColumn(nameof(PostCommentRecord.PostCommentRaw)).AsText().NotNullable()
            .WithColumn(nameof(PostCommentRecord.PostCommentMark)).AsText().Nullable()
            .WithColumn(nameof(PostCommentRecord.PostCommentUserMap)).AsText().Nullable()
            .WithReactionInfo()
            .WithColumn(nameof(PostCommentRecord.CreatedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(PostCommentRecord.TotalReportCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentRecord.DeletedTimeStamp)).AsInt64().WithDefaultValue(0);

        Create.Table(PostTagRecord.TableName)
            .WithColumn(nameof(PostTagRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostTagRecord.TagKind)).AsByte().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(PostTagRecord.TagType)).AsByte().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(PostTagRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostTagRecord.CommentId)).AsInt32().WithDefaultValue(null).ForeignKey(PostCommentRecord.TableName, Id).OnDelete(Rule.Cascade).Nullable().Indexed()
            .WithColumn(nameof(PostTagRecord.TagId)).AsInt32().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(PostTagRecord.TagString)).AsText().Indexed()
            .WithColumn(nameof(PostTagRecord.TotalReportCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.CreatedTime)).AsDateTimeOffsetWithDefault();

        Create.Table(PostReactionRecord.TableName)
            .WithColumn(nameof(PostReactionRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostReactionRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostReactionRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostReactionRecord.Reaction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostReactionRecord.LastUpdateTime)).AsDateTimeOffsetWithDefault();

        Create.Table(PostCommentReactionRecord.TableName)
            .WithColumn(nameof(PostCommentReactionRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentReactionRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostCommentReactionRecord.CommentId)).AsInt32().WithDefaultValue(0).ForeignKey(PostCommentRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostCommentReactionRecord.Reaction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentReactionRecord.LastUpdateTime)).AsDateTimeOffsetWithDefault();

        Create.Table(PostReportRecord.TableName)
            .WithColumn(nameof(PostReportRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostReportRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostReportRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostReportRecord.Reason)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostReportRecord.ReasonText)).AsText().Nullable()
            .WithColumn(nameof(PostReportRecord.CreatedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(PostReportRecord.ModifiedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(PostReportRecord.ResolvedByAccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable();

        Create.Table(PostCommentReportRecord.TableName)
            .WithColumn(nameof(PostCommentReportRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentReportRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostCommentReportRecord.CommentId)).AsInt32().ForeignKey(PostCommentRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostCommentReportRecord.Reason)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentReportRecord.ReasonText)).AsText().Nullable()
            .WithColumn(nameof(PostCommentReportRecord.CreatedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(PostCommentReportRecord.ModifiedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(PostCommentReportRecord.ResolvedByAccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable();

        Create.Table(PostTagReportRecord.TableName)
            .WithColumn(nameof(PostTagReportRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostTagReportRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostTagReportRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostTagReportRecord.TagId)).AsInt32().WithDefaultValue(0).ForeignKey(PostTagRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(PostTagReportRecord.CreatedTime)).AsDateTimeOffsetWithDefault()
            .WithColumn(nameof(PostTagReportRecord.ResolvedByAccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable();

        Create.Table(AccountHistoryRecord.TableName)
            .WithColumn(nameof(AccountHistoryRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountHistoryRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(AccountHistoryRecord.Type)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountHistoryRecord.OtherAccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.TargetId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(AccountHistoryRecord.TargetPostId)).AsInt32().ForeignKey(PostRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.TargetCommentId)).AsInt32().ForeignKey(PostCommentRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.CreatedTime)).AsDateTimeOffsetWithDefault();

        Create.Table(AccountUploadLog.TableName)
            .WithColumn(nameof(AccountUploadLog.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountUploadLog.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(AccountUploadLog.PostId)).AsInt32().ForeignKey(PostRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable().Indexed()
            .WithColumn(nameof(AccountUploadLog.BlobName)).AsText().Unique().NotNullable()
            .WithColumn(nameof(AccountUploadLog.BlobHash)).AsText().WithDefaultValue(0)
            .WithColumn(nameof(AccountUploadLog.UploadStatus)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountUploadLog.UploadTime)).AsDateTimeOffsetWithDefault();
    }

    public override void Down()
    {
        Delete.Table(BlizzardUpdateChildRecord.TableName);
        Delete.Table(BlizzardUpdateRecord.TableName);

        Delete.Table(AccountUploadLog.TableName);
        Delete.Table(AccountHistoryRecord.TableName);

        Delete.Table(PostReportRecord.TableName);
        Delete.Table(PostCommentReportRecord.TableName);
        Delete.Table(PostTagReportRecord.TableName);

        Delete.Table(PostTagRecord.TableName);
        Delete.Table(PostCommentReactionRecord.TableName);
        Delete.Table(PostCommentRecord.TableName);

        Delete.Table(PostReactionRecord.TableName);
        Delete.Table(PostRecord.TableName);

        Delete.Table(CharacterAchievementRecord.TableName);
        Delete.Table(CharacterRecord.TableName);

        Delete.Table(GuildAchievementRecord.TableName);
        Delete.Table(GuildRecord.TableName);

        Delete.Table(AuthTokenRecord.TableName);

        Delete.Table(AccountFollowingRecord.TableName);
        Delete.Table(AccountRecord.TableName);
    }
}