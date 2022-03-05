namespace AzerothMemories.Database
{
    internal static class ConfigHelpers
    {
        public static bool IncludeBlizzardData = true;

        public static readonly bool MigrateDown = true;
        public static readonly bool ClearVersionDatabase = true;

        public static void WriteWithColors(ConsoleColor background, ConsoleColor foreground, string message)
        {
            var prevBackground = Console.BackgroundColor;
            var prevForeground = Console.ForegroundColor;

            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;

            Console.WriteLine(message);

            Console.BackgroundColor = prevBackground;
            Console.ForegroundColor = prevForeground;
        }

        public static bool SaftyCheck(bool option, string message)
        {
            if (!option)
            {
                return false;
            }

            WriteWithColors(ConsoleColor.White, ConsoleColor.Red, message);

            var readLine = Console.ReadLine();
            var result = readLine != null && readLine.ToLower() == "yes";
            if (result)
            {
                WriteWithColors(ConsoleColor.White, ConsoleColor.Green, $"{message} == TRUE");
            }
            else
            {
                WriteWithColors(ConsoleColor.White, ConsoleColor.DarkBlue, $"{message} == FALSE");
            }

            return result;
        }
    }
}