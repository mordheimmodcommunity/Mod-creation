using System.Collections.Generic;

public class AiProfileIdComparer : IEqualityComparer<AiProfileId>
{
    public static readonly AiProfileIdComparer Instance = new AiProfileIdComparer();

    public bool Equals(AiProfileId x, AiProfileId y)
    {
        return x == y;
    }

    public int GetHashCode(AiProfileId obj)
    {
        return (int)obj;
    }
}
