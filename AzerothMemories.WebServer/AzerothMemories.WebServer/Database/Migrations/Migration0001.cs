using FluentMigrator;

namespace AzerothMemories.WebServer.Database.Migrations;

[Migration(1)]
public sealed class Migration0001 : Migration
{
    public override void Up()
    {
        Create.Table("Accounts")
            .WithColumn(nameof(AccountRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(AccountRecord.FusionId)).AsString(60).Unique().NotNullable()
            .WithColumn(nameof(AccountRecord.AccountType)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountRecord.BattleTag)).AsString(60).Nullable()
            .WithColumn(nameof(AccountRecord.BattleTagIsPublic)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(AccountRecord.BattleNetToken)).AsString(200).Nullable()
            .WithColumn(nameof(AccountRecord.BattleNetTokenExpiresAt)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(AccountRecord.Username)).AsString(60).Unique().Nullable()
            .WithColumn(nameof(AccountRecord.UsernameSearchable)).AsString(60).Nullable()
            .WithColumn(nameof(AccountRecord.IsPrivate)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(AccountRecord.Avatar)).AsString(200).Nullable()
            .WithColumn(nameof(AccountRecord.SocialDiscord)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.SocialTwitter)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.SocialTwitch)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.SocialYouTube)).AsString(50).Nullable()
            .WithColumn(nameof(AccountRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable()
            .WithUpdateJobInfo();

        Create.Table("Accounts_Following")
            .WithColumn(nameof(AccountFollowingRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(AccountFollowingRecord.AccountId)).AsInt64().ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade).Nullable()
            .WithColumn(nameof(AccountFollowingRecord.FollowerId)).AsInt64().ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade).Nullable()
            .WithColumn(nameof(AccountFollowingRecord.Status)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountFollowingRecord.LastUpdateTime)).AsDateTimeOffset().NotNullable()
            .WithColumn(nameof(AccountFollowingRecord.CreatedTime)).AsDateTimeOffset().NotNullable();

        Create.Table("Guilds")
            .WithColumn(nameof(GuildRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(GuildRecord.MoaRef)).AsString(128).Unique().NotNullable()
            .WithColumn(nameof(GuildRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.Name)).AsString(60).Nullable()
            .WithColumn(nameof(GuildRecord.NameSearchable)).AsString(60).Nullable()
            .WithColumn(nameof(GuildRecord.RealmId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.Faction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.MemberCount)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.AchievementPoints)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable()
            .WithColumn(nameof(GuildRecord.BlizzardCreatedTimestamp)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.BlizzardProfileLastModified)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.BlizzardAchievementsLastModified)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(GuildRecord.BlizzardRosterLastModified)).AsInt64().WithDefaultValue(0)
            .WithUpdateJobInfo();

        Create.Table("Characters")
            .WithColumn(nameof(CharacterRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterRecord.MoaRef)).AsString(128).Unique().NotNullable()
            .WithColumn(nameof(CharacterRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.Name)).AsString(60).Nullable()
            .WithColumn(nameof(CharacterRecord.NameSearchable)).AsString(60).Nullable()
            .WithColumn(nameof(CharacterRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable()
            .WithColumn(nameof(CharacterRecord.AccountId)).AsInt64().ForeignKey("Accounts", "Id").OnDelete(Rule.SetNull).Nullable()
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
            .WithColumn(nameof(CharacterRecord.GuildId)).AsInt64().ForeignKey("Guilds", "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(CharacterRecord.GuildRef)).AsString(128).Nullable()
            .WithColumn(nameof(CharacterRecord.BlizzardGuildId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardGuildRank)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardGuildName)).AsString(60).Nullable()
            .WithColumn(nameof(CharacterRecord.BlizzardProfileLastModified)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardRendersLastModified)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterRecord.BlizzardAchievementsLastModified)).AsInt64().WithDefaultValue(0)
            .WithUpdateJobInfo();

        Create.Table("Characters_Achievements")
            .WithColumn(nameof(CharacterAchievementRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterAchievementRecord.AccountId)).AsInt64().ForeignKey("Accounts", "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(CharacterAchievementRecord.CharacterId)).AsInt64().ForeignKey("Characters", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(CharacterAchievementRecord.AchievementId)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(CharacterAchievementRecord.AchievementTimeStamp)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(CharacterAchievementRecord.CompletedByCharacter)).AsBoolean().WithDefaultValue(false);

        //Create.Table("Blizzard_Data")
        //    .WithColumn(nameof(BlizzardDataRecord.Id)).AsInt64().PrimaryKey().Identity()
        //    .WithColumn(nameof(BlizzardDataRecord.TagId)).AsInt64()
        //    .WithColumn(nameof(BlizzardDataRecord.TagType)).AsByte()
        //    .WithColumn(nameof(BlizzardDataRecord.Key)).AsString(128).Unique().NotNullable()
        //    .WithColumn(nameof(BlizzardDataRecord.Media)).AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.En_Us).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Ko_Kr).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Fr_Fr).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.De_De).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Zh_Cn).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Es_Es).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Zh_Tw).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.En_Gb).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Es_Mx).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Ru_Ru).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Pt_Br).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.It_It).Replace("_", string.Empty)}").AsString(250).Nullable()
        //    .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.Pt_Pt).Replace("_", string.Empty)}").AsString(250).Nullable();

        Create.Table("Posts")
            .WithColumn(nameof(PostRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(PostRecord.AccountId)).AsInt64().WithDefaultValue(0).ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostRecord.PostComment)).AsString(2048).Nullable()
            .WithColumn(nameof(PostRecord.PostAvatar)).AsString(256).Nullable()
            .WithColumn(nameof(PostRecord.PostVisibility)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.PostFlags)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.BlobNames)).AsString(2048)
            .WithColumn(nameof(PostRecord.PostTime)).AsDateTimeOffset()
            .WithColumn(nameof(PostRecord.PostEditedTime)).AsDateTimeOffset()
            .WithColumn(nameof(PostRecord.PostCreatedTime)).AsDateTimeOffset()
            .WithReactionInfo()
            .WithColumn(nameof(PostRecord.TotalCommentCount)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.TotalReportCount)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.DeletedTimeStamp)).AsInt64().WithDefaultValue(0);

        Create.Table("Posts_Comments")
            .WithColumn(nameof(PostCommentRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentRecord.AccountId)).AsInt64().WithDefaultValue(0).ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentRecord.PostId)).AsInt64().WithDefaultValue(0).ForeignKey("Posts", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentRecord.ParentId)).AsInt64().ForeignKey("Posts_Comments", "Id").OnDelete(Rule.Cascade).Nullable()
            .WithColumn(nameof(PostCommentRecord.PostComment)).AsString(2048).NotNullable()
            .WithReactionInfo()
            .WithColumn(nameof(PostCommentRecord.CreatedTime)).AsDateTimeOffset()
            .WithColumn(nameof(PostCommentRecord.TotalReportCount)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentRecord.DeletedTimeStamp)).AsInt64().WithDefaultValue(0);

        Create.Table("Posts_Tags")
            .WithColumn(nameof(PostTagRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(PostTagRecord.TagKind)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.TagType)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.PostId)).AsInt64().WithDefaultValue(0).ForeignKey("Posts", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostTagRecord.CommentId)).AsInt64().WithDefaultValue(null).ForeignKey("Posts_Comments", "Id").OnDelete(Rule.Cascade).Nullable()
            .WithColumn(nameof(PostTagRecord.TagId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(PostTagRecord.TagString)).AsString(128)
            .WithColumn(nameof(PostTagRecord.CreatedTime)).AsDateTimeOffset();

        Create.Table("Posts_Reactions")
            .WithColumn(nameof(PostReactionRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(PostReactionRecord.AccountId)).AsInt64().WithDefaultValue(0).ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostReactionRecord.PostId)).AsInt64().WithDefaultValue(0).ForeignKey("Posts", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostReactionRecord.Reaction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostReactionRecord.LastUpdateTime)).AsDateTimeOffset();

        Create.Table("Posts_Comments_Reactions")
            .WithColumn(nameof(PostCommentReactionRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(PostCommentReactionRecord.AccountId)).AsInt64().WithDefaultValue(0).ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentReactionRecord.CommentId)).AsInt64().WithDefaultValue(0).ForeignKey("Posts_Comments", "Id").OnDelete(Rule.Cascade)
            .WithColumn(nameof(PostCommentReactionRecord.Reaction)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(PostCommentReactionRecord.LastUpdateTime)).AsDateTimeOffset();

        Create.Table("Accounts_History")
            .WithColumn(nameof(AccountHistoryRecord.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(AccountHistoryRecord.AccountId)).AsInt64().ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.Type)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(AccountHistoryRecord.OtherAccountId)).AsInt64().ForeignKey("Accounts", "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.TargetId)).AsInt64().WithDefaultValue(0)
            .WithColumn(nameof(AccountHistoryRecord.TargetPostId)).AsInt64().ForeignKey("Posts", "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.TargetCommentId)).AsInt64().ForeignKey("Posts_Comments", "Id").OnDelete(Rule.SetNull).Nullable()
            .WithColumn(nameof(AccountHistoryRecord.CreatedTime)).AsDateTimeOffset().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Accounts_History");

        Delete.Table("Posts_Tags");
        Delete.Table("Posts_Comments_Reactions");
        Delete.Table("Posts_Comments");

        Delete.Table("Posts_Reactions");
        Delete.Table("Posts");

        Delete.Table("Characters_Achievements");
        Delete.Table("Characters");

        Delete.Table("Guilds");

        Delete.Table("Accounts_Following");
        Delete.Table("Accounts");

        //Delete.Table("Blizzard_Data");
    }
}