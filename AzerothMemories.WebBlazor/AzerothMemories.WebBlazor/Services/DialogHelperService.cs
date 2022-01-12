namespace AzerothMemories.WebBlazor.Services;

public sealed class DialogHelperService
{
    private readonly IDialogService _dialogService;

    public DialogHelperService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public void ShowLoadingDialog()
    {
    }

    public void HideLoadingDialog()
    {
    }
}