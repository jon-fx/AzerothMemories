namespace AzerothMemories.WebBlazor.ViewModels;

public static class AccountPermissionExt
{
    public static readonly AccountType Permission_CanUploadAvatar = AccountType.Tier1;
    public static readonly AccountType Permission_CanChangeSocialLinks = AccountType.Tier2;

    public static bool IsAdmin(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.AccountType >= AccountType.Admin;
    }

    public static bool CanModifyFollowing(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanModifyAvatar(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanUploadAvatar(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        if (accountViewModel.AccountType < Permission_CanUploadAvatar)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanChangeSocialLinks(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.AccountType >= Permission_CanChangeSocialLinks;
    }

    public static bool CanAddMemory(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanReactToPost(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanPublishComment(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanReactToComment(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanRestoreMemory(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanUpdateSystemTags(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanDeleteAnyPost(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanDeleteAnyComment(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanChangeAnyPostVisibility(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanUpdateSystemTagsOnAnyPost(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanChangeAnyUsersAvatar(this AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }
}