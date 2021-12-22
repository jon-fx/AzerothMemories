namespace AzerothMemories.WebServer.Database.Records
{
    public interface IGrainRecord
    {
        long Id { get; set; }

        string MoaRef { get; set; }
    }
}