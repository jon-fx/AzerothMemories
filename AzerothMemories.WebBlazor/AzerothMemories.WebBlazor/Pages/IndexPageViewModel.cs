namespace AzerothMemories.WebBlazor.Pages;

public sealed class IndexPageViewModel : ViewModelBase
{
    public ActiveAccountViewModel AccountViewModel { get; private set; }

    public override async Task ComputeState()
    {
        AccountViewModel = await Services.AccountServices.TryGetAccount(null);
    }
}