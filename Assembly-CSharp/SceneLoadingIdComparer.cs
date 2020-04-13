using System.Collections.Generic;

public class SceneLoadingIdComparer : IEqualityComparer<SceneLoadingId>
{
    public static readonly SceneLoadingIdComparer Instance = new SceneLoadingIdComparer();

    public bool Equals(SceneLoadingId x, SceneLoadingId y)
    {
        return x == y;
    }

    public int GetHashCode(SceneLoadingId obj)
    {
        return (int)obj;
    }
}
