using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class BlizzardUpdateRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Updates";

    [Key] public int Id { get; init; }

    [Column] public int? AccountId { get; init; }

    [Column] public AccountRecord? Account { get; init; }

    [Column] public int? CharacterId { get; init; }

    [Column] public CharacterRecord? Character { get; init; }

    [Column] public int? GuildId { get; init; }

    [Column] public GuildRecord? Guild { get; init; }

    [Column] public Instant UpdateLastModified { get; set; }

    [Column] public Instant UpdateJobLastEndTime { get; set; }

    [Column] public BlizzardUpdateStatus UpdateStatus { get; set; }

    [Column] public byte UpdatePriority { get; set; }

    public ICollection<BlizzardUpdateChildRecord>? Children { get; init; }

    public ICommand<HttpStatusCode> GetUpdateCommand()
    {
        if (AccountId != null)
        {
            return new Updates_UpdateAccountCommand(AccountId.Value);
        }

        if (CharacterId != null)
        {
            return new Updates_UpdateCharacterCommand(CharacterId.Value);
        }

        if (GuildId != null)
        {
            return new Updates_UpdateGuildCommand(GuildId.Value);
        }

        throw new NotImplementedException();
    }

    public BlizzardUpdateViewModel? GetUpdateJobResults()
    {
        if (Children == null)
        {
            return null;
        }

        var children = Children.Select(x => new BlizzardUpdateViewModelChild
        {
            Id = x.Id,
            UpdateType = (byte)x.UpdateType,
            UpdateTypeString = x.UpdateTypeString,
            UpdateJobLastResult = x.UpdateJobLastResult
        });

        return new BlizzardUpdateViewModel
        {
            Children = children.OrderBy(x => x.UpdateType).ToArray(),
            UpdateLastModified = UpdateLastModified.ToUnixTimeMilliseconds(),
            UpdateJobLastEndTime = UpdateJobLastEndTime.ToUnixTimeMilliseconds()
        };
    }
}