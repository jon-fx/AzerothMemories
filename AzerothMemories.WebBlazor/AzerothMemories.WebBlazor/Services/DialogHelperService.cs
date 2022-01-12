using AzerothMemories.WebBlazor.Components.Dialogs;

namespace AzerothMemories.WebBlazor.Services;

public sealed class DialogHelperService
{
    private readonly IDialogService _dialogService;

    private bool _loadingDialog;
    private IDialogReference _currentDialog;

    public DialogHelperService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public void ShowLoadingDialog()
    {
        if (_currentDialog != null)
        {
            throw new NotImplementedException();
        }

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            CloseButton = false,
            DisableBackdropClick = true,
            NoHeader = true
        };

        _loadingDialog = true;
        _currentDialog = _dialogService.Show<LoadingDialog>("Loading...", options);
    }

    public void HideLoadingDialog()
    {
        if (!_loadingDialog)
        {
            throw new NotImplementedException();
        }

        _currentDialog?.Close();
        _currentDialog = null;
        _loadingDialog = false;
    }

    public async Task<bool?> ShowMessageBox(string title, string message = null, string yesText = null, string noText = null, string cancelText = null, DialogOptions options = null)
    {
        var result = await _dialogService.ShowMessageBox(title, message, yesText, noText, cancelText, options);

        return result;
    }
}