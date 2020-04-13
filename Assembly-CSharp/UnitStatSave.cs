using System.Collections.Generic;
using System.IO;

public class UnitStatSave : IThoth
{
    private int lastVersion;

    public string name;

    public string overrideName;

    public int id;

    public Dictionary<int, int> stats;

    public List<Tuple<int, EventLogger.LogEvent, int>> history;

    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(overrideName) || PandoraSingleton<Hephaestus>.Instance.IsPrivilegeRestricted(Hephaestus.RestrictionId.UGC))
            {
                return name;
            }
            return overrideName;
        }
    }

    public UnitStatSave()
    {
        name = string.Empty;
        overrideName = string.Empty;
        id = 0;
        stats = new Dictionary<int, int>();
        history = new List<Tuple<int, EventLogger.LogEvent, int>>();
    }

    int IThoth.GetVersion()
    {
        return 11;
    }

    void IThoth.Write(BinaryWriter writer)
    {
        Thoth.Write(writer, ((IThoth)this).GetVersion());
        int cRC = GetCRC(read: false);
        Thoth.Write(writer, cRC);
        Thoth.Write(writer, name);
        Thoth.Write(writer, id);
        Thoth.Write(writer, stats.Count);
        foreach (KeyValuePair<int, int> stat in stats)
        {
            Thoth.Write(writer, stat.Key);
            Thoth.Write(writer, stat.Value);
        }
        Thoth.Write(writer, history.Count);
        for (int i = 0; i < history.Count; i++)
        {
            Thoth.Write(writer, history[i].Item1);
            Thoth.Write(writer, (int)history[i].Item2);
            Thoth.Write(writer, history[i].Item3);
        }
        Thoth.Write(writer, overrideName);
    }

    void IThoth.Read(BinaryReader reader)
    {
        int i = 0;
        Thoth.Read(reader, out int i2);
        lastVersion = i2;
        if (i2 > 8)
        {
            Thoth.Read(reader, out i);
        }
        if (i2 > 7)
        {
            Thoth.Read(reader, out name);
            Thoth.Read(reader, out id);
        }
        int i4;
        if (i2 < 5)
        {
            Thoth.Read(reader, out int i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            if (i2 >= 3)
            {
                Thoth.Read(reader, out i3);
                Thoth.Read(reader, out i3);
            }
        }
        else if (i2 < 7)
        {
            Thoth.Read(reader, out i4);
            for (int j = 0; j < i4; j++)
            {
                Thoth.Read(reader, out bool b);
                if (b)
                {
                    Thoth.Read(reader, out int i5);
                    stats[j] = i5;
                }
            }
        }
        else
        {
            Thoth.Read(reader, out i4);
            for (int k = 0; k < i4; k++)
            {
                Thoth.Read(reader, out int i6);
                Thoth.Read(reader, out int i7);
                stats[i6] = i7;
            }
        }
        if (i2 >= 4)
        {
            Thoth.Read(reader, out i4);
            for (int l = 0; l < i4; l++)
            {
                int i8 = 0;
                int i9 = 0;
                int i10 = 0;
                Thoth.Read(reader, out i8);
                Thoth.Read(reader, out i9);
                Thoth.Read(reader, out i10);
                history.Add(new Tuple<int, EventLogger.LogEvent, int>(i8, (EventLogger.LogEvent)i9, i10));
            }
        }
        if (i2 > 10)
        {
            Thoth.Read(reader, out overrideName);
        }
    }

    public int GetCRC(bool read)
    {
        return CalculateCRC(read);
    }

    private int CalculateCRC(bool read)
    {
        int num = (!read) ? ((IThoth)this).GetVersion() : lastVersion;
        int num2 = 0;
        char[] array = name.ToCharArray();
        for (int i = 0; i < array.Length; i++)
        {
            num2 += array[i];
        }
        num2 += id;
        foreach (KeyValuePair<int, int> stat in stats)
        {
            num2 += stat.Value + stat.Key;
        }
        for (int j = 0; j < history.Count; j++)
        {
            num2 = (int)(num2 + (history[j].Item1 + history[j].Item2 + history[j].Item3));
        }
        if (num > 10 && !string.IsNullOrEmpty(overrideName))
        {
            array = overrideName.ToCharArray();
            for (int k = 0; k < array.Length; k++)
            {
                num2 += array[k];
            }
        }
        return num2;
    }
}
