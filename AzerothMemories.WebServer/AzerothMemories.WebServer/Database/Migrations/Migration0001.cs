﻿using FluentMigrator;

namespace AzerothMemories.WebServer.Database.Migrations
{
    [Migration(1)]
    public sealed class Migration0001 : Migration
    {
        public override void Up()
        {
            Create.Table("Accounts")
                .WithColumn(nameof(AccountRecord.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(AccountRecord.FusionId)).AsString(60).Unique().NotNullable()
                .WithColumn(nameof(AccountRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable()
                .WithColumn(nameof(AccountRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
                .WithColumn(nameof(AccountRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
                .WithColumn(nameof(AccountRecord.BattleTag)).AsString(60).Nullable()
                .WithColumn(nameof(AccountRecord.BattleTagIsPublic)).AsBoolean().WithDefaultValue(false)
                .WithColumn(nameof(AccountRecord.BattleNetToken)).AsString(200).Nullable()
                .WithColumn(nameof(AccountRecord.BattleNetTokenExpiresAt)).AsDateTimeOffset().Nullable()
                .WithColumn(nameof(AccountRecord.Username)).AsString(60).Unique().Nullable()
                .WithColumn(nameof(AccountRecord.UsernameSearchable)).AsString(60).Nullable()
                .WithColumn(nameof(AccountRecord.IsPrivate)).AsBoolean().WithDefaultValue(false)
                .WithColumn(nameof(AccountRecord.Avatar)).AsString(100).Nullable()
                .WithUpdateJobInfo();

            Create.Table("Characters")
                .WithColumn(nameof(CharacterRecord.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(CharacterRecord.MoaRef)).AsString(128).Unique().NotNullable()
                .WithColumn(nameof(CharacterRecord.BlizzardId)).AsInt64().WithDefaultValue(0)
                .WithColumn(nameof(CharacterRecord.BlizzardRegionId)).AsByte().WithDefaultValue(0)
                .WithColumn(nameof(CharacterRecord.Name)).AsString(60).Nullable()
                .WithColumn(nameof(CharacterRecord.NameSearchable)).AsString(60).Nullable()
                .WithColumn(nameof(CharacterRecord.CreatedDateTime)).AsDateTimeOffset().NotNullable()
                .WithColumn(nameof(CharacterRecord.AccountId)).AsInt64().WithDefaultValue(0)
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
                .WithColumn(nameof(CharacterRecord.GuildId)).AsInt64().WithDefaultValue(0)
                .WithColumn(nameof(CharacterRecord.GuildRank)).AsByte().WithDefaultValue(0)
                .WithColumn(nameof(CharacterRecord.GuildName)).AsString(60).Nullable()
                .WithColumn(nameof(CharacterRecord.GuildRef)).AsString(60).Nullable()
                .WithColumn(nameof(CharacterRecord.BlizzardProfileLastModified)).AsInt64().WithDefaultValue(0)
                .WithColumn(nameof(CharacterRecord.BlizzardRendersLastModified)).AsInt64().WithDefaultValue(0)
                .WithColumn(nameof(CharacterRecord.BlizzardAchievementsLastModified)).AsInt64().WithDefaultValue(0)
                .WithUpdateJobInfo();

            Create.Table("Characters_Achievements")
                .WithColumn(nameof(CharacterAchievementRecord.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(CharacterAchievementRecord.AccountId)).AsInt64().WithDefaultValue(0).ForeignKey("Accounts", "Id")
                .WithColumn(nameof(CharacterAchievementRecord.CharacterId)).AsInt64().WithDefaultValue(0).ForeignKey("Characters", "Id")
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
        }

        public override void Down()
        {
            Delete.Table("Characters_Achievements");

            Delete.Table("Accounts");
            Delete.Table("Characters");

            //Delete.Table("Blizzard_Data");
        }
    }
}