using System.Text;

namespace AzerothMemories.WebBlazor.Common;

public static class DatabaseHelpers
{
    public static bool IsValidAccountName(string username)
    {
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

        return asciiStr;
    }
}