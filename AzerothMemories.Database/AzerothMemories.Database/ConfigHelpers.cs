namespace AzerothMemories.Database
{
    internal static class ConfigHelpers
    {
        public static readonly bool MigrateDown = true;
        public static readonly bool ClearVersionDatabase = true;

        public static readonly bool SkipBlizzardData = true;

        
        private static readonly Stack<ConsoleColor> _backgroundColors = new();
        private static readonly Stack<ConsoleColor> _foregroundColors = new();

        public static void PushColors(ConsoleColor back, ConsoleColor fore)
        {
            _backgroundColors.Push(Console.BackgroundColor );
            _foregroundColors.Push(Console.ForegroundColor );

            Console.BackgroundColor = back;
            Console.ForegroundColor = fore;
        }

        public static void PopColors()
        {
            Console.BackgroundColor = _backgroundColors.Pop( );
            Console.ForegroundColor =  _foregroundColors.Pop();
        }
    }
}