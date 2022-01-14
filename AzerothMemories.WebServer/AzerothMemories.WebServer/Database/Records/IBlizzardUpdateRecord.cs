namespace AzerothMemories.WebServer.Database.Records;

public interface IBlizzardUpdateRecord : IDatabaseRecord
{
    string UpdateJob { get; set; }

    //Instant? UpdateJobQueueTime { get; set; }

    //Instant? UpdateJobStartTime { get; set; }

    Instant UpdateJobEndTime { get; set; }

    HttpStatusCode UpdateJobLastResult { get; set; }
}