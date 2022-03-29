using AzerothMemories.WebBlazor.Components.Dialogs;

namespace AzerothMemories.WebBlazor.Services;

public sealed class DialogHelperService
{
    private readonly IDialogService _dialogService;
    private readonly List<IDialogReference> _activeDialogs;
    private IDialogReference _loadingDialog;

    public DialogHelperService(IDialogService dialogService)
    {
        _dialogService = dialogService;
        _activeDialogs = new List<IDialogReference>();
    }

    public void ShowLoadingDialog()
    {
        if (_loadingDialog != null)
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

        _loadingDialog = _dialogService.Show<LoadingDialog>("Loading...", options);
    }

    public void HideLoadingDialog()
    {
        if (_loadingDialog == null)
        {
            throw new NotImplementedException();
        }

        _loadingDialog.Close();
        _loadingDialog = null;
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

        await ShowDialog<NotificationDialog>("Notification", parameters, options);
    }

    public async Task<DialogResult> ShowReportPostDialog(string message, int postId, int commentId)
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

    public async Task<DialogResult> ShowAdminUserDialog(string title, int accountId)
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        };

        var parameters = new DialogParameters
        {
            ["accountId"] = accountId,
        };

        var result = await ShowDialog<AdminUserDialog>(title, parameters, options);
        return result;
    }

    public async Task<bool?> ShowMessageBox(string title, string message = null, string yesText = null, string noText = null, string cancelText = null, DialogOptions options = null)
    {
        var result = await _dialogService.ShowMessageBox(title, message, yesText, noText, cancelText, options);

        return result;
    }

    private async Task<DialogResult> ShowDialog<TDialog>(string title, DialogParameters dialogParameters, DialogOptions options) where TDialog : ComponentBase
    {
        var currentDialog = _dialogService.Show<TDialog>(title, dialogParameters, options);

        _activeDialogs.Add(currentDialog);

        var result = await currentDialog.Result;

        _activeDialogs.Remove(currentDialog);

        return result;
    }
}