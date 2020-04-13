using System.Collections.Generic;

public class AssetExtensionIdComparer : IEqualityComparer<AssetExtensionId>
{
    public static readonly AssetExtensionIdComparer Instance = new AssetExtensionIdComparer();

    public bool Equals(AssetExtensionId x, AssetExtensionId y)
    {
        return x == y;
    }

    public int GetHashCode(AssetExtensionId obj)
    {
        return (int)obj;
    }
}
