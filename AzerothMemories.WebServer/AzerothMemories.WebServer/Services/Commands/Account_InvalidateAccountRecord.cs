namespace AzerothMemories.WebServer.Services.Commands;

public record Account_InvalidateAccountRecord(int Id, string Username, string FusionId)
{
    public Account_InvalidateAccountRecord() : this(0, null, null)
    {
    }
}