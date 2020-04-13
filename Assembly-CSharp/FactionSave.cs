using System.Collections.Generic;
using System.IO;

public class FactionSave : IThoth
{
    private int lastVersion;

    public FactionId factionId;

    public int reputation;

    public int rank;

    public int factionIndex;

    public List<ShipmentSave> shipments;

    public FactionSave()
    {
        factionId = FactionId.NONE;
        reputation = 0;
        rank = 0;
        factionIndex = 0;
        shipments = new List<ShipmentSave>();
    }

    public FactionSave(FactionId id, int index)
    {
        factionId = id;
        reputation = 0;
        rank = 0;
        factionIndex = index;
        shipments = new List<ShipmentSave>();
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
        Thoth.Write(writer, (int)factionId);
        Thoth.Write(writer, reputation);
        Thoth.Write(writer, rank);
        Thoth.Write(writer, factionIndex);
        Thoth.Write(writer, shipments.Count);
        for (int i = 0; i < shipments.Count; i++)
        {
            ShipmentSave shipmentSave = shipments[i];
            Thoth.Write(writer, shipmentSave.weight);
            ShipmentSave shipmentSave2 = shipments[i];
            Thoth.Write(writer, shipmentSave2.gold);
            ShipmentSave shipmentSave3 = shipments[i];
            Thoth.Write(writer, shipmentSave3.rank);
            ShipmentSave shipmentSave4 = shipments[i];
            Thoth.Write(writer, shipmentSave4.sendDate);
            ShipmentSave shipmentSave5 = shipments[i];
            Thoth.Write(writer, shipmentSave5.guid);
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
        int i3 = 0;
        Thoth.Read(reader, out i3);
        factionId = (FactionId)i3;
        Thoth.Read(reader, out reputation);
        Thoth.Read(reader, out rank);
        Thoth.Read(reader, out factionIndex);
        if (i2 <= 2)
        {
            return;
        }
        Thoth.Read(reader, out int i4);
        ShipmentSave item = default(ShipmentSave);
        for (int j = 0; j < i4; j++)
        {
            Thoth.Read(reader, out item.weight);
            Thoth.Read(reader, out item.gold);
            Thoth.Read(reader, out item.rank);
            item.sendDate = 0;
            item.guid = 0;
            if (i2 > 3)
            {
                Thoth.Read(reader, out item.sendDate);
                Thoth.Read(reader, out item.guid);
            }
            shipments.Add(item);
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
        num2 = (int)(num2 + factionId);
        num2 += reputation;
        num2 += rank;
        num2 += factionIndex;
        for (int i = 0; i < shipments.Count; i++)
        {
            int num3 = num2;
            ShipmentSave shipmentSave = shipments[i];
            num2 = num3 + shipmentSave.weight;
            int num4 = num2;
            ShipmentSave shipmentSave2 = shipments[i];
            num2 = num4 + shipmentSave2.gold;
            int num5 = num2;
            ShipmentSave shipmentSave3 = shipments[i];
            num2 = num5 + shipmentSave3.rank;
            int num6 = num2;
            ShipmentSave shipmentSave4 = shipments[i];
            num2 = num6 + shipmentSave4.sendDate;
            int num7 = num2;
            ShipmentSave shipmentSave5 = shipments[i];
            num2 = num7 + shipmentSave5.guid;
        }
        return num2;
    }
}
