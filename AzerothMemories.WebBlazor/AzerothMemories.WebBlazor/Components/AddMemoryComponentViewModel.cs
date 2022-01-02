namespace AzerothMemories.WebBlazor.Components;

public sealed class AddMemoryComponentViewModel : ViewModelBase
{
    public AddMemoryComponentViewModel()
    {
        UploadResults = new List<AddMemoryUploadResult>();
    }

    public override Task ComputeState(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public List<AddMemoryUploadResult> UploadResults { get; }

    public AddMemoryComponentSharedData SharedData { get; set; }

    public bool MaxUploadReached => UploadResults.Count > 3;
}