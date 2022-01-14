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

    public async Task ShowNotificationDialog(bool success, string message)
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            CloseButton = true,
            NoHeader = true
        };

        var parameters = new DialogParameters
        {
            ["success"] = success,
            ["message"] = message
        };

        await ShowDialog<NotificationDialog>("Report Post", parameters, options);
    }

    public async Task<DialogResult> ShowReportPostDialog(string message, long postId, long commentId)
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        };

        var parameters = new DialogParameters
        {
            ["postid"] = postId,
            ["commentid"] = commentId
        };

        var result = await ShowDialog<ReportPostDialog>(message, parameters, options);
        return result;
    }

    public async Task<DialogResult> ShowReportPostTagsDialog(string title, PostViewModel viewModel)
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        };

        var parameters = new DialogParameters
        {
            ["post"] = viewModel,
        };

        var result = await ShowDialog<ReportPostTagsDialog>(title, parameters, options);
        return result;
    }

    public async Task<bool?> ShowMessageBox(string title, string message = null, string yesText = null, string noText = null, string cancelText = null, DialogOptions options = null)
    {
        var result = await _dialogService.ShowMessageBox(title, message, yesText, noText, cancelText, options);

        return result;
    }

    private async Task<DialogResult> ShowDialog<TDialog>(string title, DialogParameters dialogParameters = null, DialogOptions options = null) where TDialog : ComponentBase
    {
        if (_currentDialog != null || _loadingDialog)
        {
            throw new NotImplementedException();
        }

        _currentDialog = _dialogService.Show<TDialog>(title, dialogParameters, options);

        var result = await _currentDialog.Result.ConfigureAwait(true);

        _currentDialog = null;

        return result;
    }
}