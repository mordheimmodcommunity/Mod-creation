using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class Constant
{
    private static readonly Dictionary<int, float> cachedFloat;

    private static readonly Dictionary<int, int> cachedInt;

    private static readonly Dictionary<int, Color> cachedColor;

    private static readonly string[] constants;

    private static readonly string[] intConstants;

    private static readonly string[] negIntConstants;

    private static readonly string[] negPercConstants;

    private static readonly string[] percConstants;

    static Constant()
    {
        cachedFloat = new Dictionary<int, float>();
        cachedInt = new Dictionary<int, int>();
        cachedColor = new Dictionary<int, Color>();
        constants = new string[159];
        intConstants = new string[2000];
        negIntConstants = new string[2000];
        negPercConstants = new string[101];
        percConstants = new string[101];
        for (int i = 0; i < intConstants.Length; i++)
        {
            intConstants[i] = i.ToString();
            negIntConstants[i] = (-i).ToString();
        }
        for (int j = 0; j <= 100; j++)
        {
            percConstants[j] = j + "%";
            negPercConstants[j] = -j + "%";
        }
    }

    public static void Init()
    {
        List<ConstantData> list = PandoraSingleton<DataFactory>.Instance.InitData<ConstantData>();
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            constants[(int)list[i].Id] = list[i].Value;
        }
    }

    public static int GetInt(ConstantId constant)
    {
        if (!cachedInt.TryGetValue((int)constant, out int value))
        {
            value = int.Parse(constants[(int)constant]);
            cachedInt[(int)constant] = value;
        }
        return value;
    }

    public static float GetFloat(ConstantId constant)
    {
        if (!cachedFloat.TryGetValue((int)constant, out float value))
        {
            value = float.Parse(constants[(int)constant], NumberFormatInfo.InvariantInfo);
            cachedFloat[(int)constant] = value;
        }
        return value;
    }

    public static float GetFloatSqr(ConstantId constant)
    {
        float @float = GetFloat(constant);
        return @float * @float;
    }

    public static string GetString(ConstantId constant)
    {
        return constants[(int)constant];
    }

    public static Color GetColor(ConstantId constant)
    {
        if (!cachedColor.TryGetValue((int)constant, out Color value))
        {
            string[] array = constants[(int)constant].Split(new char[1]
            {
                ','
            });
            value = new Color((float)Convert.ToInt32(array[0]) / 255f, (float)Convert.ToInt32(array[1]) / 255f, (float)Convert.ToInt32(array[2]) / 255f, (array.Length <= 3) ? 1f : ((float)Convert.ToInt32(array[3]) / 255f));
            cachedColor[(int)constant] = value;
        }
        return value;
    }

    public static string ToString(int value)
    {
        int num = Math.Abs(value);
        if (num >= 0 && num < 2000)
        {
            if (value >= 0)
            {
                return intConstants[value];
            }
            return negIntConstants[num];
        }
        return value.ToString();
    }

    public static string ToPercString(int value)
    {
        if (value >= -100 && value <= 100)
        {
            if (value >= 0)
            {
                return percConstants[value];
            }
            return negPercConstants[-value];
        }
        return PandoraUtils.StringBuilder.Append(value.ToConstantString()).Append('%').ToString();
    }

    public static string ToConstantString(this int value)
    {
        return ToString(value);
    }

    public static string ToConstantPercString(this int value)
    {
        return ToPercString(value);
    }
}
