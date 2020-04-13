using System.Collections.Generic;

public static class ListExtensions
{
    public static void SafeAddRange<T>(this List<T> self, IEnumerable<T> toAdd)
    {
        if (toAdd != null)
        {
            self.AddRange(toAdd);
        }
    }

    public static bool Contains<T>(this List<T> self, T elmt, IEqualityComparer<T> comparer)
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (comparer.Equals(self[i], elmt))
            {
                return true;
            }
        }
        return false;
    }

    public static int IndexOf<T>(this List<T> self, T elmt, IEqualityComparer<T> comparer)
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (comparer.Equals(self[i], elmt))
            {
                return i;
            }
        }
        return -1;
    }
}
