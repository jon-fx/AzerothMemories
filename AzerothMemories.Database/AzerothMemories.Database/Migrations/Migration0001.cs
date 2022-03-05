using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;
using System.Data;

namespace AzerothMemories.Database.Migrations;

[Migration(1)]
public sealed class Migration0001 : Migration
{
    private readonly bool SkipBlizzardData = true;

    public override void Up()
    {
        Create.Table(AccountRecord.TableName)
            .WithColumn(nameof(AccountRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountRecord.FusionId)).AsString(60).Unique().NotNullable()
            .WithColumn(nameof(AccountRecord.AccountType)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.AccountFlags)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BattleTag)).AsString(60).Nullable()
            .WithColumn(nameof(AccountRecord.BattleTagIsPublic)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(AccountRecord.BattleNetToken)).AsString(200).Nullable()
            .WithColumn(nameof(AccountRecord.BattleNetTokenExpiresAt)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(AccountRecord.Username)).AsString(60).Unique().Nullable()
            .WithColumn(nameof(AccountRecord.UsernameSearchable)).AsString(60).Nullable()
            .WithColumn(nameof(AccountRecord.UsernameChangedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(AccountRecord.IsPrivate)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(AccountRecord.Avatar)).AsString(200).Nullable()
            .WithColumn(nameof(AccountRecord.SocialDiscord)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.SocialTwitter)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.SocialTwitch)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.SocialYouTube)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.LastLoginTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(AccountRecord.LoginConsecutiveDaysCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(AccountRecord.BanReason)).AsString(200).Nullable()
            .WithColumn(nameof(AccountRecord.BanExpireTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithUpdateJobInfo();

        Create.Table(AccountFollowingRecord.TableName)
            .WithColumn(nameof(AccountFollowingRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountFollowingRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(AccountFollowingRecord.FollowerId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(AccountFollowingRecord.Status)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountFollowingRecord.LastUpdateTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(AccountFollowingRecord.CreatedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch);

        Create.Table(GuildRecord.TableName)
            .WithColumn(nameof(GuildRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(GuildRecord.MoaRef)).AsString(128).Unique().NotNullable()
            .WithColumn(nameof(GuildRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.Name)).AsString(60).Nullable()
            .WithColumn(nameof(GuildRecord.NameSearchable)).AsString(60).Nullable()
            .WithColumn(nameof(GuildRecord.RealmId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.Faction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.MemberCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.AchievementPoints)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(GuildRecord.BlizzardCreatedTimestamp)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(GuildRecord.BlizzardProfileLastModified)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(GuildRecord.BlizzardAchievementsLastModified)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(GuildRecord.BlizzardRosterLastModified)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithUpdateJobInfo();

        Create.Table(CharacterRecord.TableName)
            .WithColumn(nameof(CharacterRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterRecord.MoaRef)).AsString(128).Unique().NotNullable()
            .WithColumn(nameof(CharacterRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Name)).AsString(60).Nullable()
            .WithColumn(nameof(CharacterRecord.NameSearchable)).AsString(60).Nullable()
            .WithColumn(nameof(CharacterRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(CharacterRecord.CharacterStatus)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(CharacterRecord.AccountSync)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(CharacterRecord.RealmId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Class)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Race)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Gender)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Level)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Faction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.AvatarLink)).AsString(128).Nullable()
            .WithColumn(nameof(CharacterRecord.AchievementTotalPoints)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.AchievementTotalQuantity)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.GuildId)).AsInt32().ForeignKey(GuildRecord.TableName, "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(CharacterRecord.GuildRef)).AsString(128).Nullable()
            //.WithColumn(nameof(CharacterRecord.BlizzardGuildId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardGuildRank)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardGuildName)).AsString(60).Nullable()
            .WithColumn(nameof(CharacterRecord.BlizzardProfileLastModified)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(CharacterRecord.BlizzardRendersLastModified)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(CharacterRecord.BlizzardAchievementsLastModified)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithUpdateJobInfo();

        Create.Table(CharacterAchievementRecord.TableName)
            .WithColumn(nameof(CharacterAchievementRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterAchievementRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(CharacterAchievementRecord.CharacterId)).AsInt32().ForeignKey(CharacterRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(CharacterAchievementRecord.AchievementId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterAchievementRecord.AchievementTimeStamp)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(CharacterAchievementRecord.CompletedByCharacter)).AsBoolean().WithDefaultValue(false);

        if (SkipBlizzardData)
        {
        }
        else
        {
            Create.Table(BlizzardDataRecord.TableName)
                .WithColumn(nameof(BlizzardDataRecord.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(BlizzardDataRecord.TagType)).AsByte()
                .WithColumn(nameof(BlizzardDataRecord.TagId)).AsInt32()
                .WithColumn(nameof(BlizzardDataRecord.Key)).AsString(128).Unique().NotNullable()
                .WithColumn(nameof(BlizzardDataRecord.Media)).AsString(250).Nullable()
                .WithColumn(nameof(BlizzardDataRecord.MinTagTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnUs)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.KoKr)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.FrFr)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.DeDe)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhCn)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsEs)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhTw)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnGb)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsMx)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.RuRu)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtBr)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ItIt)}").AsString(250).Nullable()
                .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtPt)}").AsString(250).Nullable();

            var indexNames = new[]
            {
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnUs)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.KoKr)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.FrFr)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.DeDe)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhCn)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsEs)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhTw)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnGb)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsMx)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.RuRu)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtBr)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ItIt)}",
                $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtPt)}"
            };

            foreach (var indexName in indexNames)
            {
                Execute.Sql($"CREATE INDEX IX_Blizzard_Data_{indexName} ON \"{BlizzardDataRecord.TableName}\" (LOWER(\"{indexName}\") varchar_pattern_ops)");
            }
        }

        Create.Table(PostRecord.TableName)
            .WithColumn(nameof(PostRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostRecord.PostComment)).AsString(2048).Nullable()
            .WithColumn(nameof(PostRecord.PostAvatar)).AsString(256).Nullable()
            .WithColumn(nameof(PostRecord.PostVisibility)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.PostFlags)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.BlobNames)).AsString(2048)
            .WithColumn(nameof(PostRecord.PostTime)).AsDateTimeOffset()
            .WithColumn(nameof(PostRecord.PostEditedTime)).AsDateTimeOffset()
            .WithColumn(nameof(PostRecord.PostCreatedTime)).AsDateTimeOffset()
            .WithReactionInfo()
            .WithColumn(nameof(PostRecord.TotalCommentCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.TotalReportCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.DeletedTimeStamp)).AsInt64().WithDefaultValue(0);

        Create.Table(PostCommentRecord.TableName)
            .WithColumn(nameof(PostCommentRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentRecord.ParentId)).AsInt32().ForeignKey(PostCommentRecord.TableName, "Id").OnDelete(Rule.Cascade).Nullable()
            .WithColumn(nameof(PostCommentRecord.PostComment)).AsString(2048).NotNullable()
            .WithReactionInfo()
            .WithColumn(nameof(PostCommentRecord.CreatedTime)).AsDateTimeOffset()
            .WithColumn(nameof(PostCommentRecord.TotalReportCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentRecord.DeletedTimeStamp)).AsInt64().WithDefaultValue(0);

        Create.Table(PostTagRecord.TableName)
            .WithColumn(nameof(PostTagRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostTagRecord.TagKind)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.TagType)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostTagRecord.CommentId)).AsInt32().WithDefaultValue(null).ForeignKey(PostCommentRecord.TableName, "Id").OnDelete(Rule.Cascade).Nullable()
            .WithColumn(nameof(PostTagRecord.TagId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.TagString)).AsString(128)
            .WithColumn(nameof(PostTagRecord.TotalReportCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.CreatedTime)).AsDateTimeOffset();

        Create.Table(PostReactionRecord.TableName)
            .WithColumn(nameof(PostReactionRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostReactionRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostReactionRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostReactionRecord.Reaction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostReactionRecord.LastUpdateTime)).AsDateTimeOffset();

        Create.Table(PostCommentReactionRecord.TableName)
            .WithColumn(nameof(PostCommentReactionRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentReactionRecord.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentReactionRecord.CommentId)).AsInt32().WithDefaultValue(0).ForeignKey(PostCommentRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentReactionRecord.Reaction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentReactionRecord.LastUpdateTime)).AsDateTimeOffset();

        Create.Table(PostReportRecord.TableName)
            .WithColumn(nameof(PostReportRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostReportRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostReportRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostReportRecord.Reason)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostReportRecord.ReasonText)).AsString(200).Nullable()
            .WithColumn(nameof(PostReportRecord.CreatedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(PostReportRecord.ModifiedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch);

        Create.Table(PostCommentReportRecord.TableName)
            .WithColumn(nameof(PostCommentReportRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentReportRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentReportRecord.CommentId)).AsInt32().ForeignKey(PostCommentRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentReportRecord.Reason)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentReportRecord.ReasonText)).AsString(200).Nullable()
            .WithColumn(nameof(PostCommentReportRecord.CreatedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch)
            .WithColumn(nameof(PostCommentReportRecord.ModifiedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch);

        Create.Table(PostTagReportRecord.TableName)
            .WithColumn(nameof(PostTagReportRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PostTagReportRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostTagReportRecord.PostId)).AsInt32().WithDefaultValue(0).ForeignKey(PostRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostTagReportRecord.TagId)).AsInt32().WithDefaultValue(0).ForeignKey(PostTagRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostTagReportRecord.CreatedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch);

        Create.Table(AccountHistoryRecord.TableName)
            .WithColumn(nameof(AccountHistoryRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountHistoryRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(AccountHistoryRecord.Type)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountHistoryRecord.OtherAccountId)).AsInt32().ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.TargetId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(AccountHistoryRecord.TargetPostId)).AsInt32().ForeignKey(PostRecord.TableName, "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.TargetCommentId)).AsInt32().ForeignKey(PostCommentRecord.TableName, "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.CreatedTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch);

        Create.Table(AccountUploadLog.TableName)
            .WithColumn(nameof(AccountUploadLog.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(AccountUploadLog.AccountId)).AsInt32().WithDefaultValue(0).ForeignKey(AccountRecord.TableName, "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(AccountUploadLog.PostId)).AsInt32().ForeignKey(PostRecord.TableName, "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountUploadLog.BlobName)).AsString(128).Unique().NotNullable()
            .WithColumn(nameof(AccountUploadLog.BlobHash)).AsString(64).WithDefaultValue(0)
            .WithColumn(nameof(AccountUploadLog.UploadStatus)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountUploadLog.UploadTime)).AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch);

        Create.Index().OnTable(AccountRecord.TableName)
            .OnColumn(nameof(AccountRecord.UsernameSearchable));

        Create.Index().OnTable(AccountFollowingRecord.TableName)
            .OnColumn(nameof(AccountFollowingRecord.AccountId));

        Create.Index().OnTable(AccountFollowingRecord.TableName)
            .OnColumn(nameof(AccountFollowingRecord.FollowerId));

        Create.Index().OnTable(AccountUploadLog.TableName)
            .OnColumn(nameof(AccountUploadLog.AccountId));

        Create.Index().OnTable(AccountHistoryRecord.TableName)
            .OnColumn(nameof(AccountHistoryRecord.AccountId));

        Create.Index().OnTable(CharacterRecord.TableName)
            .OnColumn(nameof(CharacterRecord.AccountId));

        Create.Index().OnTable(CharacterRecord.TableName)
            .OnColumn(nameof(CharacterRecord.NameSearchable));

        Create.Index().OnTable(CharacterRecord.TableName)
            .OnColumn(nameof(CharacterRecord.GuildId));

        Create.Index().OnTable(CharacterAchievementRecord.TableName)
            .OnColumn(nameof(CharacterAchievementRecord.AccountId));

        Create.Index().OnTable(CharacterAchievementRecord.TableName)
            .OnColumn(nameof(CharacterAchievementRecord.CharacterId));

        Create.Index().OnTable(CharacterAchievementRecord.TableName)
            .OnColumn(nameof(CharacterAchievementRecord.AchievementId));

        Create.Index().OnTable(CharacterAchievementRecord.TableName)
            .OnColumn(nameof(CharacterAchievementRecord.AchievementTimeStamp));

        Create.Index().OnTable(GuildRecord.TableName)
            .OnColumn(nameof(GuildRecord.NameSearchable));

        Create.Index().OnTable(PostRecord.TableName)
            .OnColumn(nameof(PostRecord.AccountId));

        Create.Index().OnTable(PostRecord.TableName)
            .OnColumn(nameof(PostRecord.PostTime));

        Create.Index().OnTable(PostRecord.TableName)
            .OnColumn(nameof(PostRecord.PostCreatedTime));

        Create.Index().OnTable(PostRecord.TableName)
            .OnColumn(nameof(PostRecord.DeletedTimeStamp));

        Create.Index().OnTable(PostCommentRecord.TableName)
            .OnColumn(nameof(PostCommentRecord.AccountId));

        Create.Index().OnTable(PostCommentRecord.TableName)
            .OnColumn(nameof(PostCommentRecord.PostId));

        Create.Index().OnTable(PostCommentRecord.TableName)
            .OnColumn(nameof(PostCommentRecord.ParentId));

        Create.Index().OnTable(PostCommentReactionRecord.TableName)
            .OnColumn(nameof(PostCommentReactionRecord.AccountId));

        Create.Index().OnTable(PostCommentReactionRecord.TableName)
            .OnColumn(nameof(PostCommentReactionRecord.CommentId));

        Create.Index().OnTable(PostCommentReportRecord.TableName)
            .OnColumn(nameof(PostCommentReportRecord.AccountId));

        Create.Index().OnTable(PostCommentReportRecord.TableName)
            .OnColumn(nameof(PostCommentReportRecord.CommentId));

        Create.Index().OnTable(PostReactionRecord.TableName)
            .OnColumn(nameof(PostReactionRecord.AccountId));

        Create.Index().OnTable(PostReactionRecord.TableName)
            .OnColumn(nameof(PostReactionRecord.PostId));

        Create.Index().OnTable(PostReportRecord.TableName)
            .OnColumn(nameof(PostReportRecord.AccountId));

        Create.Index().OnTable(PostReportRecord.TableName)
            .OnColumn(nameof(PostReportRecord.PostId));

        Create.Index().OnTable(PostTagReportRecord.TableName)
            .OnColumn(nameof(PostTagReportRecord.AccountId));

        Create.Index().OnTable(PostTagReportRecord.TableName)
            .OnColumn(nameof(PostTagReportRecord.PostId));

        Create.Index().OnTable(PostTagReportRecord.TableName)
            .OnColumn(nameof(PostTagReportRecord.TagId));

        Create.Index().OnTable(PostTagRecord.TableName)
            .OnColumn(nameof(PostTagRecord.PostId));

        Create.Index().OnTable(PostTagRecord.TableName)
            .OnColumn(nameof(PostTagRecord.CommentId));

        Create.Index().OnTable(PostTagRecord.TableName)
            .OnColumn(nameof(PostTagRecord.TagKind));

        Create.Index().OnTable(PostTagRecord.TableName)
            .OnColumn(nameof(PostTagRecord.TagType));

        Create.Index().OnTable(PostTagRecord.TableName)
            .OnColumn(nameof(PostTagRecord.TagId));

        Create.Index().OnTable(PostTagRecord.TableName)
            .OnColumn(nameof(PostTagRecord.TagString));
    }

    public override void Down()
    {
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

        Delete.Table(GuildRecord.TableName);

        Delete.Table(AccountFollowingRecord.TableName);
        Delete.Table(AccountRecord.TableName);

        if (SkipBlizzardData)
        {
        }
        else
        {
            Delete.Table(BlizzardDataRecord.TableName);
        }
    }
}