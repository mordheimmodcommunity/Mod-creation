using System;
using System.Collections.Generic;

public class Faction
{
    public FactionData Data
    {
        get;
        private set;
    }

    public FactionSave Save
    {
        get;
        private set;
    }

    public WarbandSave WarSave
    {
        get;
        private set;
    }

    public bool Primary
    {
        get;
        set;
    }

    public int Rank => Save.rank;

    public List<FactionRankData> RanksData
    {
        get;
        private set;
    }

    public List<FactionRankWarbandSkillData> Rewards
    {
        get;
        set;
    }

    public int Reputation => Save.reputation;

    public string LocalizedConsequences
    {
        get;
        private set;
    }

    public Faction(WarbandSave warSave, FactionSave save)
    {
        Data = PandoraSingleton<DataFactory>.Instance.InitData<FactionData>((int)save.factionId);
        WarSave = warSave;
        Save = save;
        Primary = Data.Primary;
        RanksData = PandoraSingleton<DataFactory>.Instance.InitData<FactionRankData>();
        Rewards = PandoraSingleton<DataFactory>.Instance.InitData<FactionRankWarbandSkillData>("fk_faction_id", ((int)Data.Id).ToString());
    }

    public WarbandSkillId GetRewardWarbandSkillId(FactionRankId rankId)
    {
        for (int i = 0; i < Rewards.Count; i++)
        {
            if (Rewards[i].FactionRankId == rankId)
            {
                return Rewards[i].WarbandSkillId;
            }
        }
        return WarbandSkillId.NONE;
    }

    public bool HasRank(int rank)
    {
        for (int i = 0; i < Save.shipments.Count; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                ShipmentSave shipmentSave = Save.shipments[i];
                int num = (shipmentSave.rank >> 4 * j) & 0xF;
                if (num > 0 && num == rank)
                {
                    return false;
                }
            }
        }
        return Rank >= rank;
    }

    public EventLogger.LogEvent GetFactionDeliveryEvent()
    {
        EventLogger.LogEvent result = EventLogger.LogEvent.NONE;
        switch (Save.factionIndex)
        {
            case 0:
                result = EventLogger.LogEvent.FACTION0_DELIVERY;
                break;
            case 1:
                result = EventLogger.LogEvent.FACTION1_DELIVERY;
                break;
            case 2:
                result = EventLogger.LogEvent.FACTION2_DELIVERY;
                break;
        }
        return result;
    }

    public bool SaveDelivery(int weight, int gold, int rank, int date, out int id)
    {
        ShipmentSave shipmentSave2 = default(ShipmentSave);
        for (int i = 0; i < Save.shipments.Count; i++)
        {
            ShipmentSave shipmentSave = Save.shipments[i];
            if (shipmentSave.sendDate != date)
            {
                continue;
            }
            shipmentSave2 = Save.shipments[i];
            shipmentSave2.weight += weight;
            shipmentSave2.gold += gold;
            if (rank > 0)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (shipmentSave2.rank >> 4 * j == 0)
                    {
                        shipmentSave2.rank |= rank << 4 * j;
                        break;
                    }
                }
            }
            id = shipmentSave2.guid;
            Save.shipments[i] = shipmentSave2;
            return false;
        }
        shipmentSave2.weight = weight;
        shipmentSave2.gold = gold;
        shipmentSave2.rank = rank;
        shipmentSave2.sendDate = date;
        shipmentSave2.guid = Guid.NewGuid().GetHashCode();
        Save.shipments.Add(shipmentSave2);
        id = shipmentSave2.guid;
        return true;
    }

    public ShipmentSave GetDelivery(int guid)
    {
        for (int i = 0; i < Save.shipments.Count; i++)
        {
            ShipmentSave shipmentSave = Save.shipments[i];
            if (shipmentSave.guid == guid)
            {
                return Save.shipments[i];
            }
        }
        return default(ShipmentSave);
    }

    public void ClearDelivery(int guid)
    {
        int num = Save.shipments.Count - 1;
        while (true)
        {
            if (num >= 0)
            {
                ShipmentSave shipmentSave = Save.shipments[num];
                if (shipmentSave.guid == guid)
                {
                    break;
                }
                num--;
                continue;
            }
            return;
        }
        Save.shipments.RemoveAt(num);
    }
}
