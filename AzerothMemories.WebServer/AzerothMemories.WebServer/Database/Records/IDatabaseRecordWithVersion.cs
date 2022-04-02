namespace AzerothMemories.WebServer.Database.Records;

public interface IDatabaseRecordWithVersion : IDatabaseRecord
{
    uint RowVersion { get; set; }
}