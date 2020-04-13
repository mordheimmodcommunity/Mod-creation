using System;

public static class KGFCoreVersion
{
    private static Version itsVersion = new Version(1, 2, 0, 0);

    public static Version GetCurrentVersion()
    {
        return itsVersion.Clone() as Version;
    }
}
