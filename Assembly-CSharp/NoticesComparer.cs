using System.Collections.Generic;

public class NoticesComparer : IEqualityComparer<Notices>
{
    public static readonly NoticesComparer Instance = new NoticesComparer();

    public bool Equals(Notices x, Notices y)
    {
        return x == y;
    }

    public int GetHashCode(Notices obj)
    {
        return (int)obj;
    }
}
