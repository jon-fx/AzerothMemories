namespace AzerothMemories.Database;

internal static class ConfigHelpers
{
    public static void WriteWithColors(ConsoleColor foreground, string message)
    {
        var prevForeground = Console.ForegroundColor;

        Console.ForegroundColor = foreground;

        Console.WriteLine(message);

        Console.ForegroundColor = prevForeground;
    }

    public static bool SafetyCheck(string message)
    {
        WriteWithColors(ConsoleColor.Red, message);

        var readLine = Console.ReadLine();
        var result = readLine != null && readLine.ToLower() == "yes";
        if (result)
        {
            WriteWithColors(ConsoleColor.Green, $"{message} == TRUE");
        }
        else
        {
            WriteWithColors(ConsoleColor.Yellow, $"{message} == FALSE");
        }

        return result;
    }
}