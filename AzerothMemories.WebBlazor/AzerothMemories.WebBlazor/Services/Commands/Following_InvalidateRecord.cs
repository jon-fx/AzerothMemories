namespace AzerothMemories.WebBlazor.Services.Commands;

public record Following_InvalidateRecord(long AccountId, long OtherAccountId)
{
    public Following_InvalidateRecord() : this(0, 0)
    {
    }
}