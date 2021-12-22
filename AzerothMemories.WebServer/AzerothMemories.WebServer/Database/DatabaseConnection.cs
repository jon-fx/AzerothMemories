using AzerothMemories.WebServer.Database.Records;

namespace AzerothMemories.WebServer.Database
{
    public sealed class DatabaseConnection : DataConnection
    {
        public DatabaseConnection(LinqToDbConnectionOptions databaseConfig) : base(databaseConfig)
        {
        }

        public ITable<AccountGrainRecord> Accounts => GetTable<AccountGrainRecord>();

        public ITable<CharacterGrainRecord> Characters => GetTable<CharacterGrainRecord>();

        public ITable<CharacterAchievementRecord> CharacterAchievements => GetTable<CharacterAchievementRecord>();

        public ITable<GuildGrainRecord> Guilds => GetTable<GuildGrainRecord>();
    }
}