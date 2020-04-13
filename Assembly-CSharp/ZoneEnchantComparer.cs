using System.Collections.Generic;

public class ZoneEnchantComparer : IComparer<ZoneAoeEnchantmentData>
{
    int IComparer<ZoneAoeEnchantmentData>.Compare(ZoneAoeEnchantmentData x, ZoneAoeEnchantmentData y)
    {
        if (x.ZoneTriggerId > y.ZoneTriggerId)
        {
            return 1;
        }
        if (x.ZoneTriggerId < y.ZoneTriggerId)
        {
            return -1;
        }
        return 0;
    }
}
