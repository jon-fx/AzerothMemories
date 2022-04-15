using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class BlizzardUpdateRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Updates";

    [Key] public int Id { get; set; }

    [Column] public int? AccountId { get; set; }

    [Column] public AccountRecord Account { get; set; }

    [Column] public int? CharacterId { get; set; }

    [Column] public CharacterRecord Character { get; set; }

    [Column] public int? GuildId { get; set; }

    [Column] public GuildRecord Guild { get; set; }

    [Column] public Instant UpdateLastModified { get; set; }

    [Column] public Instant UpdateJobLastEndTime { get; set; }

    [Column] public BlizzardUpdateStatus UpdateStatus { get; set; }

    [Column] public BlizzardUpdatePriority UpdatePriority { get; set; }

    public ICollection<BlizzardUpdateChildRecord> Children { get; set; }

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

    public BlizzardUpdateViewModel GetUpdateJobResults()
    {
        if (Children == null)
        {
            return null;
        }

        var children = Children.Select(x => new BlizzardUpdateViewModelChild()
        {
            Id = x.Id,
            UpdateType = (byte) x.UpdateType,
            UpdateTypeString = x.UpdateTypeString,
            UpdateJobLastResult = x.UpdateJobLastResult,
        });

        return new BlizzardUpdateViewModel
        {
            Children = children.OrderBy(x => x.UpdateType).ToArray(),
            UpdateLastModified = UpdateLastModified.ToUnixTimeMilliseconds(),
            UpdateJobLastEndTime = UpdateJobLastEndTime.ToUnixTimeMilliseconds(),
        };
    }
}