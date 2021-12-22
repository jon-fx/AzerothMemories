using System.Net;

namespace AzerothMemories.WebServer.Database.Records
{
    public interface IBlizzardGrainUpdateRecord : IGrainRecord
    {
        DateTimeOffset LastUpdateEndTime { get; set; }

        RequestResultCode LastUpdateResult { get; set; }

        HttpStatusCode LastUpdateHttpResult { get; set; }
    }
}