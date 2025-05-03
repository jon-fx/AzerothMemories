using AzerothMemories.WebBlazor.Components.Dialogs;
using System.Diagnostics.CodeAnalysis;

namespace AzerothMemories.WebBlazor.Services;

public sealed class DialogHelperService
{
    private readonly IDialogService _dialogService;
    private readonly List<IDialogReference> _activeDialogs;
    private IDialogReference? _loadingDialog;

    public DialogHelperService(IDialogService dialogService)
    {
        _dialogService = dialogService;
        _activeDialogs = [];
    }

    public bool IsLoadingDialogVisible => _loadingDialog != null;

    public async Task ShowLoadingDialog()
    {
        if (_loadingDialog != null)
        {
            throw new NotImplementedException();
        }

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            CloseButton = false,
            BackdropClick = false,
            NoHeader = true
        };

        _loadingDialog = await _dialogService.ShowAsync<LoadingDialog>("Loading...", options);
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

    public async Task ShowReportPostDialog(string message, int postId, int commentId)
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

        await ShowDialog<ReportPostDialog>(message, parameters, options);
    }

    public async Task ShowReportPostTagsDialog(string title, PostViewModel viewModel)
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        };

        var parameters = new DialogParameters
        {
            ["post"] = viewModel
        };

        await ShowDialog<ReportPostTagsDialog>(title, parameters, options);
    }

    public async Task ShowAdminUserDialog(string title, int accountId)
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        };

        var parameters = new DialogParameters
        {
            ["accountId"] = accountId
        };

        await ShowDialog<AdminUserDialog>(title, parameters, options);
    }

    public async Task<bool?> ShowMessageBox(string title, string message, string? yesText = null, string? noText = null, string? cancelText = null, DialogOptions? options = null)
    {
        var result = await _dialogService.ShowMessageBox(title, message, yesText ?? "OK", noText, cancelText, options);

        return result;
    }

    private async Task ShowDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDialog>(
        string title, DialogParameters dialogParameters, DialogOptions options) where TDialog : ComponentBase
    {
        var currentDialog = await _dialogService.ShowAsync<TDialog>(title, dialogParameters, options);

        _activeDialogs.Add(currentDialog);

        await currentDialog.Result;

        _activeDialogs.Remove(currentDialog);
    }
}