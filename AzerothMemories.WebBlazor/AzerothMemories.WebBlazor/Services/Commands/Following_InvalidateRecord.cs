namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_InvalidateRecord(int AccountId, int OtherAccountId)
{
    public Following_InvalidateRecord() : this(0, 0)
    {
    }
}