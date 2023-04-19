using System.Text;

namespace AzerothMemories.WebBlazor.Common;

public static class DatabaseHelpers
{
    private static readonly HashSet<string> _badUsernames = new(StringComparer.OrdinalIgnoreCase)
    {
        "abuse",
        "account",
        "adm",
        "admin",
        "admins",
        "administrator",
        "administrators",
        "all",
        "ceo",
        "cfo",
        "contact",
        "coo",
        "customer",
        "document",
        "documents",
        "download",
        "downloads",
        "faq",
        "file",
        "files",
        "ftp",
        "help",
        "home",
        "host_master",
        "host-master",
        "hostmaster",
        "http",
        "https",
        "imap",
        "info",
        "ldap",
        "list",
        "list-request",
        "mail",
        "majordomo",
        "manager",
        "marketing",
        "member",
        "membership",
        "mis",
        "news",
        "noreply",
        "office",
        "owner",
        "password",
        "pop",
        "post_master",
        "post-master",
        "postfix",
        "postmaster",
        "register",
        "registration",
        "root",
        "sales",
        "secure",
        "security",
        "sftp",
        "site",
        "shop",
        "smtp",
        "ssl",
        "ssl_admin",
        "ssl-admin",
        "ssladmin",
        "ssl_administrator",
        "ssl-administrator",
        "ssladministrator",
        "ssl_webmaster",
        "ssl-webmaster",
        "sslwebmaster",
        "support",
        "sysadmin",
        "test",
        "trouble",
        "usenet",
        "user",
        "username",
        "users",
        "web",
        "web_master",
        "web-master",
        "webmaster",
        "web_admin",
        "web-admin",
        "webadmin",
        "webmail",
        "webserver",
        "website",
        "wheel",
        "vww",
        "wvw",
        "wwv",
        "www",
        "www-data",
        "wwww"
    };

    public static bool IsValidAccountName(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        if (username.Length < 3)
        {
            return false;
        }

        if (username.Length > 50)
        {
            return false;
        }

        if (!char.IsLetter(username[0]))
        {
            return false;
        }

        if (_badUsernames.Contains(username))
        {
            return false;
        }

        foreach (var c in username)
        {
            if (char.IsLetterOrDigit(c))
            {
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public static string GetSearchableName(string name)
    {
        var tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(name);
        var asciiStr = Encoding.UTF8.GetString(tempBytes);
        asciiStr = asciiStr.Replace("?", string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(asciiStr))
        {
            asciiStr = name;
        }

        return asciiStr;
    }
}