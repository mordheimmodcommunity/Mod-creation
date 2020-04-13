using System;
using System.IO;

public class Thoth
{
    public static string WriteToString(IThoth thoth)
    {
        //Discarded unreachable code: IL_0025, IL_0037
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                thoth.Write(writer);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }

    public static bool ReadFromString(string source, IThoth target)
    {
        //Discarded unreachable code: IL_0078
        bool flag = true;
        try
        {
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(source)))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    target.Read(reader);
                    return memoryStream.Length == memoryStream.Position;
                }
            }
        }
        catch (IOException ex)
        {
            PandoraDebug.LogWarning("Unable to read string: " + ex.ToString(), "THOTH");
            return false;
        }
    }

    public static byte[] WriteToArray(IThoth thoth)
    {
        //Discarded unreachable code: IL_0020, IL_0032
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                thoth.Write(writer);
                return memoryStream.ToArray();
            }
        }
    }

    public static bool ReadFromArray(byte[] bytes, IThoth thoth)
    {
        //Discarded unreachable code: IL_0085
        if (bytes == null)
        {
            return false;
        }
        if (bytes.Length == 0)
        {
            return false;
        }
        bool flag = true;
        try
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    thoth.Read(reader);
                    return memoryStream.Length == memoryStream.Position;
                }
            }
        }
        catch (IOException ex)
        {
            PandoraDebug.LogWarning("Unable to read array: " + ex.ToString(), "THOTH");
            return false;
        }
    }

    public static void WriteToFile(string filePath, IThoth thoth)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        try
        {
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    thoth.Write(writer);
                }
                stream.Close();
            }
        }
        catch (IOException ex)
        {
            PandoraDebug.LogWarning("Unable to write file: " + ex.ToString(), "THOTH");
        }
    }

    public static bool ReadFromFile(string filePath, IThoth thoth)
    {
        //Discarded unreachable code: IL_007a
        bool result = true;
        try
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    thoth.Read(reader);
                    result = (stream.Length == stream.Position);
                }
                stream.Close();
                return result;
            }
        }
        catch (IOException ex)
        {
            PandoraDebug.LogWarning("Unable to read from file: " + ex.ToString(), "THOTH");
            return false;
        }
    }

    public static void Copy(IThoth src, IThoth dst)
    {
        try
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    src.Write(writer);
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    using (BinaryReader reader = new BinaryReader(memoryStream))
                    {
                        dst.Read(reader);
                    }
                }
            }
        }
        catch (IOException ex)
        {
            PandoraDebug.LogWarning("Unable to copy file: " + ex.ToString(), "THOTH");
        }
    }

    public static void Write(BinaryWriter writer, bool b)
    {
        writer.Write(b);
    }

    public static void Write(BinaryWriter writer, byte b)
    {
        writer.Write(b);
    }

    public static void Write(BinaryWriter writer, byte[] b)
    {
        writer.Write(b.Length);
        writer.Write(b);
    }

    public static void Write(BinaryWriter writer, char c)
    {
        writer.Write(c);
    }

    public static void Write(BinaryWriter writer, char[] c)
    {
        writer.Write(c.Length);
        writer.Write(c);
    }

    public static void Write(BinaryWriter writer, decimal d)
    {
        writer.Write(d);
    }

    public static void Write(BinaryWriter writer, double d)
    {
        writer.Write(d);
    }

    public static void Write(BinaryWriter writer, short i)
    {
        writer.Write(i);
    }

    public static void Write(BinaryWriter writer, int i)
    {
        writer.Write(i);
    }

    public static void Write(BinaryWriter writer, long i)
    {
        writer.Write(i);
    }

    public static void Write(BinaryWriter writer, sbyte b)
    {
        writer.Write(b);
    }

    public static void Write(BinaryWriter writer, float s)
    {
        writer.Write(s);
    }

    public static void Write(BinaryWriter writer, string s)
    {
        writer.Write(s);
    }

    public static void Write(BinaryWriter writer, ushort i)
    {
        writer.Write(i);
    }

    public static void Write(BinaryWriter writer, uint i)
    {
        writer.Write(i);
    }

    public static void Write(BinaryWriter writer, ulong i)
    {
        writer.Write(i);
    }

    public static void Read(BinaryReader reader, out bool b)
    {
        b = reader.ReadBoolean();
    }

    public static void Read(BinaryReader reader, out byte b)
    {
        b = reader.ReadByte();
    }

    public static void Read(BinaryReader reader, out byte[] b)
    {
        int i = 0;
        Read(reader, out i);
        b = reader.ReadBytes(i);
    }

    public static void Read(BinaryReader reader, out char c)
    {
        c = reader.ReadChar();
    }

    public static void Read(BinaryReader reader, out char[] c)
    {
        int i = 0;
        Read(reader, out i);
        c = reader.ReadChars(i);
    }

    public static void Read(BinaryReader reader, out decimal d)
    {
        d = reader.ReadDecimal();
    }

    public static void Read(BinaryReader reader, out double d)
    {
        d = reader.ReadDouble();
    }

    public static void Read(BinaryReader reader, out short i)
    {
        i = reader.ReadInt16();
    }

    public static void Read(BinaryReader reader, out int i)
    {
        i = reader.ReadInt32();
    }

    public static void Read(BinaryReader reader, out long i)
    {
        i = reader.ReadInt64();
    }

    public static void Read(BinaryReader reader, out sbyte b)
    {
        b = reader.ReadSByte();
    }

    public static void Read(BinaryReader reader, out float s)
    {
        s = reader.ReadSingle();
    }

    public static void Read(BinaryReader reader, out string s)
    {
        s = reader.ReadString();
    }

    public static void Read(BinaryReader reader, out ushort i)
    {
        i = reader.ReadUInt16();
    }

    public static void Read(BinaryReader reader, out uint i)
    {
        i = reader.ReadUInt32();
    }

    public static void Read(BinaryReader reader, out ulong i)
    {
        i = reader.ReadUInt64();
    }
}
