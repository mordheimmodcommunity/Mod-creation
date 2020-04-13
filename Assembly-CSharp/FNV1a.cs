using System.Globalization;
using System.Text;

public class FNV1a
{
    private const uint FnvBasis = 2166136261u;

    private const uint FnvPrime = 16777619u;

    public static uint ComputeHash(StringBuilder strb)
    {
        uint num = 2166136261u;
        for (int i = 0; i < strb.Length; i++)
        {
            num ^= CultureInfo.InvariantCulture.TextInfo.ToLower(strb[i]);
            num *= 16777619;
        }
        return num;
    }

    public static uint ComputeHash(string str)
    {
        uint num = 2166136261u;
        for (int i = 0; i < str.Length; i++)
        {
            num ^= CultureInfo.InvariantCulture.TextInfo.ToLower(str[i]);
            num *= 16777619;
        }
        return num;
    }
}
