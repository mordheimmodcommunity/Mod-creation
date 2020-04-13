using System.Collections.Generic;

public class ColorPresetIdComparer : IEqualityComparer<ColorPresetId>
{
    public static readonly ColorPresetIdComparer Instance = new ColorPresetIdComparer();

    public bool Equals(ColorPresetId x, ColorPresetId y)
    {
        return x == y;
    }

    public int GetHashCode(ColorPresetId obj)
    {
        return (int)obj;
    }
}
