namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_InvalidateAccountRecord(long Id, string Username, string FusionId)
{
    public Account_InvalidateAccountRecord() : this(0, null, null)
    {
    }
}