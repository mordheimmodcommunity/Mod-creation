namespace WellFired.Shared
{
    public static class OpenFactory
    {
        public static bool PlatformCanOpen()
        {
            return true;
        }

        public static IOpen CreateOpen()
        {
            return new WinOpen();
        }
    }
}
