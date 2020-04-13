using System.Collections.Generic;

public class AssetBundleIdComparer : IEqualityComparer<AssetBundleId>
{
    public static readonly AssetBundleIdComparer Instance = new AssetBundleIdComparer();

    public bool Equals(AssetBundleId x, AssetBundleId y)
    {
        return x == y;
    }

    public int GetHashCode(AssetBundleId obj)
    {
        return (int)obj;
    }
}
