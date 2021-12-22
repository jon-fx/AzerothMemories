namespace AzerothMemories.WebServer.Database
{
    public static class CheckAndChange
    {
        public static bool Check<TValue>(ref TValue currentValue, TValue newValue, ref bool changed)
        {
            if (EqualityComparer<TValue>.Default.Equals(currentValue, newValue))
            {
                return false;
            }

            changed = true;
            currentValue = newValue;

            return true;
        }
    }
}