using System.Collections.Generic;
using System.IO;
using TNet;

public static class BinaryExtensions
{
    private static Dictionary<byte, object[]> mTemp = new Dictionary<byte, object[]>();

    private static object[] GetTempBuffer(int count)
    {
        if (!mTemp.TryGetValue((byte)count, out object[] value))
        {
            value = new object[count];
            mTemp[(byte)count] = value;
        }
        return value;
    }

    public static void WriteArray(this BinaryWriter bw, params object[] objs)
    {
        bw.WriteInt(objs.Length);
        if (objs.Length != 0)
        {
            int i = 0;
            for (int num = objs.Length; i < num; i++)
            {
                bw.WriteObject(objs[i]);
            }
        }
    }

    public static object[] ReadArray(this BinaryReader reader)
    {
        int num = reader.ReadInt();
        if (num == 0)
        {
            return null;
        }
        object[] tempBuffer = GetTempBuffer(num);
        for (int i = 0; i < num; i++)
        {
            tempBuffer[i] = reader.ReadObject();
        }
        return tempBuffer;
    }

    public static object[] ReadArray(this BinaryReader reader, object obj)
    {
        int num = reader.ReadInt() + 1;
        object[] tempBuffer = GetTempBuffer(num);
        tempBuffer[0] = obj;
        for (int i = 1; i < num; i++)
        {
            tempBuffer[i] = reader.ReadObject();
        }
        return tempBuffer;
    }

    public static object[] CombineArrays(object obj, params object[] objs)
    {
        int num = objs.Length;
        object[] tempBuffer = GetTempBuffer(num + 1);
        tempBuffer[0] = obj;
        for (int i = 0; i < num; i++)
        {
            tempBuffer[i + 1] = objs[i];
        }
        return tempBuffer;
    }
}
