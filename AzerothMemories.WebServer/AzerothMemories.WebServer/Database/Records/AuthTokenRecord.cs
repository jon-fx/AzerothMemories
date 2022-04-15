using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AuthTokenRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Accounts_AuthTokens";

    [Key] public int Id { get; set; }
    
    [Column] public int? AccountId { get; set; }

    [Column] public AccountRecord Account { get; set; }

    [Column] public string Key { get; set; }

    [Column] public string Name { get; set; }

    [Column] public string Token { get; set; }

    [Column] public string RefreshToken { get; set; }

    [Column] public Instant TokenExpiresAt { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    public long IdLong => GetIdFrom(Key);

    public bool IsBlizzardAuthToken => Key.StartsWith("BattleNet");

    public bool IsPatreon => Key.StartsWith("Patreon");

    public uint RowVersion { get; set; }

    //public BlizzardRegion GetBlizzardRegionId()
    //{
    //    var nameSplit = Key.Split('/');
    //    var providerSplit = nameSplit[0].Split('-');

    //    return BlizzardRegionExt.FromName(providerSplit[1]);
    //}

    public static long GetIdFrom(string key)
    {
        var nameSplit = key.Split('/');

        long.TryParse(nameSplit[1], out var blizzardId);

        return blizzardId;
    }
}