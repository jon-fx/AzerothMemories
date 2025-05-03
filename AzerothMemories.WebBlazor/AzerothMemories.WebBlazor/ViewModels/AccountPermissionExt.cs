using System.Diagnostics.CodeAnalysis;

namespace AzerothMemories.WebBlazor.ViewModels;

public static class AccountPermissionExt
{
    public static readonly AccountType Permission_CanUploadAvatar = AccountType.Tier1;
    public static readonly AccountType Permission_CanChangeSocialLinks = AccountType.Tier2;

    public static bool IsAdmin([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.AccountType >= AccountType.Admin;
    }

    public static bool IsBanned([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return Instant.FromUnixTimeMilliseconds(accountViewModel.BanExpireTime) > SystemClock.Instance.GetCurrentInstant();
    }

    public static bool CanChangeFollowing([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanChangeAvatar([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanUploadAvatar([NotNullWhen(true)] this AccountViewModel? accountViewModel)
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

    public static bool CanChangeSocialLinks([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.AccountType >= Permission_CanChangeSocialLinks;
    }

    public static bool CanAddMemory([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanReactToPost([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanPublishComment([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanReactToComment([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanRestoreMemory([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanUpdateSystemTags([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool CanDeleteAnyPost([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanDeleteAnyComment([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanChangeAnyPostVisibility([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanUpdateSystemTagsOnAnyPost([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanChangeAnyUsersAvatar([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.IsAdmin();
    }

    public static bool CanReport([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        return accountViewModel.CanInteract;
    }

    public static bool BlizzardTokenExpired([NotNullWhen(true)] this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return false;
        }

        if (accountViewModel.BlizzardTokenExpiryTime == 0)
        {
            return false;
        }

        var instant = Instant.FromUnixTimeMilliseconds(accountViewModel.BlizzardTokenExpiryTime);
        var current = SystemClock.Instance.GetCurrentInstant();
        return current > instant.Minus(Duration.FromHours(1));
    }
}