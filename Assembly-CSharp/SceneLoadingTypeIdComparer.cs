using System.Collections.Generic;

public class SceneLoadingTypeIdComparer : IEqualityComparer<SceneLoadingTypeId>
{
    public static readonly SceneLoadingTypeIdComparer Instance = new SceneLoadingTypeIdComparer();

    public bool Equals(SceneLoadingTypeId x, SceneLoadingTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(SceneLoadingTypeId obj)
    {
        return (int)obj;
    }
}
