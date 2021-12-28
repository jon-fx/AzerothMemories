namespace AzerothMemories.WebServer.Database.Records;

public interface IBlizzardGrainUpdateRecord : IDatabaseRecord
{
    string UpdateJob { get; set; }

    DateTimeOffset? UpdateJobQueueTime { get; set; }

    DateTimeOffset? UpdateJobStartTime { get; set; }

    DateTimeOffset? UpdateJobEndTime { get; set; }

    HttpStatusCode UpdateJobLastResult { get; set; }
}