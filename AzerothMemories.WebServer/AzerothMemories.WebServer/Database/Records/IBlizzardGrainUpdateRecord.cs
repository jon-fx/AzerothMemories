namespace AzerothMemories.WebServer.Database.Records;

public interface IBlizzardGrainUpdateRecord : IDatabaseRecord
{
    string UpdateJob { get; set; }

    Instant? UpdateJobQueueTime { get; set; }

    Instant? UpdateJobStartTime { get; set; }

    Instant? UpdateJobEndTime { get; set; }

    HttpStatusCode UpdateJobLastResult { get; set; }
}