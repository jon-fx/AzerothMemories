namespace AzerothMemories.WebServer.Database.Records;

public interface IBlizzardUpdateRecord : IDatabaseRecord
{
    BlizzardUpdateRecord UpdateRecord { get; set; }
}