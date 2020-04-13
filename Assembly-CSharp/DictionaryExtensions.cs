using System.Collections.Generic;

public static class DictionaryExtensions
{
    private static List<AttributeMod> tempEmptyList = new List<AttributeMod>(0);

    public static List<TListValue> GetOrNull<TKey, TListValue>(this Dictionary<TKey, List<TListValue>> dict, TKey key)
    {
        if (!dict.TryGetValue(key, out List<TListValue> value))
        {
            return new List<TListValue>(0);
        }
        return value;
    }

    public static List<AttributeMod> GetOrNull(this Dictionary<AttributeId, List<AttributeMod>> dict, AttributeId key)
    {
        if (!dict.TryGetValue(key, out List<AttributeMod> value))
        {
            return tempEmptyList;
        }
        return value;
    }
}
