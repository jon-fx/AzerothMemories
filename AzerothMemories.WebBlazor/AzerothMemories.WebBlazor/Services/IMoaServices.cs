namespace AzerothMemories.WebBlazor.Services;

public interface IMoaServices
{
    ClientServices ClientServices { get; }

    ComputeServices ComputeServices { get; }
}