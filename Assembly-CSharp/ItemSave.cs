using System;
using System.IO;

[Serializable]
public class ItemSave : IThoth
{
    private int lastVersion;

    public int id;

    public int qualityId;

    public int runeMarkId;

    public int runeMarkQualityId;

    public int allegianceId;

    public int amount;

    public int oldSlot;

    public uint ownerMyrtilus;

    public int shots;

    public int soldAmount;

    public ItemSave(ItemId itemId, ItemQualityId quality = ItemQualityId.NORMAL, RuneMarkId runeMark = RuneMarkId.NONE, RuneMarkQualityId runeMarkQuality = RuneMarkQualityId.NONE, AllegianceId allegiance = AllegianceId.NONE, int count = 1)
    {
        id = (int)itemId;
        qualityId = (int)quality;
        runeMarkId = (int)runeMark;
        runeMarkQualityId = (int)runeMarkQuality;
        allegianceId = (int)allegiance;
        amount = ((itemId != 0) ? count : 0);
        soldAmount = 0;
        ownerMyrtilus = 0u;
        shots = 0;
        lastVersion = ((IThoth)this).GetVersion();
    }

    int IThoth.GetVersion()
    {
        return 8;
    }

    void IThoth.Write(BinaryWriter writer)
    {
        int version = ((IThoth)this).GetVersion();
        Thoth.Write(writer, version);
        int cRC = GetCRC(read: false);
        Thoth.Write(writer, cRC);
        Thoth.Write(writer, id);
        Thoth.Write(writer, qualityId);
        Thoth.Write(writer, runeMarkId);
        Thoth.Write(writer, runeMarkQualityId);
        Thoth.Write(writer, allegianceId);
        Thoth.Write(writer, amount);
        Thoth.Write(writer, oldSlot);
        Thoth.Write(writer, ownerMyrtilus);
        Thoth.Write(writer, shots);
        Thoth.Write(writer, soldAmount);
    }

    void IThoth.Read(BinaryReader reader)
    {
        int i = 0;
        Thoth.Read(reader, out int i2);
        lastVersion = i2;
        if (i2 >= 4)
        {
            Thoth.Read(reader, out i);
        }
        Thoth.Read(reader, out id);
        Thoth.Read(reader, out qualityId);
        Thoth.Read(reader, out runeMarkId);
        if (i2 >= 2)
        {
            Thoth.Read(reader, out runeMarkQualityId);
        }
        Thoth.Read(reader, out allegianceId);
        if (i2 >= 3)
        {
            Thoth.Read(reader, out amount);
        }
        if (i2 >= 5)
        {
            Thoth.Read(reader, out oldSlot);
        }
        if (i2 >= 6)
        {
            Thoth.Read(reader, out ownerMyrtilus);
        }
        if (i2 >= 7)
        {
            Thoth.Read(reader, out shots);
        }
        if (i2 >= 8)
        {
            Thoth.Read(reader, out soldAmount);
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
        num2 += id;
        num2 += qualityId;
        num2 += runeMarkId;
        num2 += runeMarkQualityId;
        num2 += allegianceId;
        num2 += amount;
        if (num >= 5)
        {
            num2 += oldSlot;
        }
        if (num >= 6)
        {
            num2 += (int)ownerMyrtilus;
        }
        if (num >= 7)
        {
            num2 += shots;
        }
        if (num >= 8)
        {
            num2 += soldAmount;
        }
        return num2;
    }
}
