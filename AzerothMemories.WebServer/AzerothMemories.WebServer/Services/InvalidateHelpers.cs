namespace AzerothMemories.WebServer.Services
{
    public static class InvalidateHelpers
    {
        public static void InvalidateRecord(CommonServices commonServices, Account_InvalidateAccountRecord invRecord)
        {
            if (invRecord == null)
            {
                return;
            }

            _ = commonServices.AccountServices.TryGetAccountRecord(invRecord.Id);
            _ = commonServices.AccountServices.TryGetAccountRecordFusionId(invRecord.FusionId);
            _ = commonServices.AccountServices.TryGetAccountRecordUsername(invRecord.Username);
            _ = commonServices.AccountServices.CreateAccountViewModel(invRecord.Id, true);
            _ = commonServices.AccountServices.CreateAccountViewModel(invRecord.Id, false);
            _ = commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, invRecord.Id);
        }

        public static void InvalidateRecord(CommonServices commonServices, Character_InvalidateCharacterRecord invRecord)
        {
            if (invRecord == null)
            {
                return;
            }

            _ = commonServices.CharacterServices.TryGetCharacterRecord(invRecord.CharacterId);

            if (invRecord.AccountId > 0)
            {
                _ = commonServices.CharacterServices.TryGetAllAccountCharacters(invRecord.AccountId);
                //_ = commonServices.CharacterServices.TryGetAllAccountCharacterIds(invRecord.AccountId);
            }

            _ = commonServices.TagServices.TryGetUserTagInfo(PostTagType.Character, invRecord.CharacterId);
        }

        public static void InvalidateRecord(CommonServices commonServices, Guild_InvalidateGuildRecord invRecord)
        {
            if (invRecord == null)
            {
                return;
            }

            _ = commonServices.GuildServices.TryGetGuildRecord(invRecord.GuildId);
            _ = commonServices.TagServices.TryGetUserTagInfo(PostTagType.Guild, invRecord.GuildId);
        }
    }
}