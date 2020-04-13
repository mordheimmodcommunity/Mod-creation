using System.Collections.Generic;
using System.IO;

public class WarbandStatSave : IThoth
{
    private int lastVersion;

    public int[] stats;

    public List<Tuple<int, EventLogger.LogEvent, int>> history;

    public WarbandStatSave()
    {
        stats = new int[63];
        history = new List<Tuple<int, EventLogger.LogEvent, int>>();
    }

    int IThoth.GetVersion()
    {
        return 5;
    }

    void IThoth.Write(BinaryWriter writer)
    {
        int version = ((IThoth)this).GetVersion();
        Thoth.Write(writer, version);
        int cRC = GetCRC(read: false);
        int num = (int)PandoraSingleton<Hephaestus>.Instance.GetUserId();
        Thoth.Write(writer, cRC + num);
        Thoth.Write(writer, stats.Length);
        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i] != 0)
            {
                Thoth.Write(writer, b: true);
                Thoth.Write(writer, stats[i]);
            }
            else
            {
                Thoth.Write(writer, b: false);
            }
        }
        Thoth.Write(writer, history.Count);
        for (int j = 0; j < history.Count; j++)
        {
            Thoth.Write(writer, history[j].Item1);
            Thoth.Write(writer, (int)history[j].Item2);
            Thoth.Write(writer, history[j].Item3);
        }
    }

    void IThoth.Read(BinaryReader reader)
    {
        int i = 0;
        Thoth.Read(reader, out int i2);
        lastVersion = i2;
        if (i2 > 4)
        {
            Thoth.Read(reader, out i);
        }
        if (i2 < 3)
        {
            Thoth.Read(reader, out int i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
            Thoth.Read(reader, out i3);
        }
        else
        {
            Thoth.Read(reader, out int i4);
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
        if (i2 >= 2)
        {
            int i6 = 0;
            Thoth.Read(reader, out i6);
            for (int k = 0; k < i6; k++)
            {
                int i7 = 0;
                int i8 = 0;
                int i9 = 0;
                Thoth.Read(reader, out i7);
                Thoth.Read(reader, out i8);
                Thoth.Read(reader, out i9);
                history.Add(new Tuple<int, EventLogger.LogEvent, int>(i7, (EventLogger.LogEvent)i8, i9));
            }
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
        for (int i = 0; i < stats.Length; i++)
        {
            num2 += stats[i];
        }
        for (int j = 0; j < history.Count; j++)
        {
            num2 = (int)(num2 + (history[j].Item1 + history[j].Item2 + history[j].Item3));
        }
        return num2;
    }
}
