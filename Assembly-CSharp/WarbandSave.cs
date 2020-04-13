using System.Collections.Generic;
using System.IO;

public class WarbandSave : IThoth
{
    public int lastVersion;

    public string overrideName;

    public string name;

    public int id;

    public int currentDate;

    public int rank;

    public int xp;

    public int scoutsSent;

    public WarbandStatSave stats;

    public List<UnitSave> units;

    public List<UnitSave> outsiders;

    public List<WarbandSkillId> skills;

    public List<UnitStatSave> oldUnits;

    public List<ItemSave> items;

    public int marketEventId;

    public List<ItemSave> marketItems;

    public List<ItemSave> addedMarketItems;

    public List<FactionSave> factions;

    public int lateShipmentCount;

    public bool lastShipmentFailed;

    public int nextShipmentExtraDays;

    public int campaignId;

    public int curCampaignIdx;

    public List<MissionSave> missions;

    public uint hideoutTutos;

    public bool smugglersMaxRankShown;

    public List<int> unitsSlotsIndex;

    public List<int> exhibitionUnitsSlotsIndex;

    public int winningStreak;

    public int warbandFaced;

    public bool inMission;

    public MissionEndDataSave endMission;

    public bool lastMissionAmbushed;

    public int availaibleRespec;

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

    public WarbandSave()
        : this(WarbandId.NONE)
    {
    }

    public WarbandSave(WarbandId warbandId)
    {
        name = PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_type_" + warbandId.ToString().ToLowerInvariant());
        overrideName = string.Empty;
        id = (int)warbandId;
        currentDate = 0;
        units = new List<UnitSave>();
        outsiders = new List<UnitSave>();
        skills = new List<WarbandSkillId>();
        oldUnits = new List<UnitStatSave>();
        items = new List<ItemSave>();
        marketItems = new List<ItemSave>();
        addedMarketItems = new List<ItemSave>();
        stats = new WarbandStatSave();
        factions = new List<FactionSave>();
        missions = new List<MissionSave>();
        lateShipmentCount = 0;
        lastShipmentFailed = false;
        nextShipmentExtraDays = 0;
        campaignId = 0;
        curCampaignIdx = 1;
        scoutsSent = 0;
        marketEventId = 0;
        hideoutTutos = 0u;
        smugglersMaxRankShown = false;
        unitsSlotsIndex = new List<int>();
        exhibitionUnitsSlotsIndex = new List<int>();
        winningStreak = 0;
        warbandFaced = 0;
        inMission = false;
        endMission = new MissionEndDataSave();
        lastMissionAmbushed = false;
        availaibleRespec = -1;
    }

    int IThoth.GetVersion()
    {
        return 34;
    }

    void IThoth.Write(BinaryWriter writer)
    {
        int version = ((IThoth)this).GetVersion();
        Thoth.Write(writer, version);
        int cRC = GetCRC(read: false);
        int num = (int)PandoraSingleton<Hephaestus>.Instance.GetUserId();
        Thoth.Write(writer, cRC + num);
        Thoth.Write(writer, name);
        Thoth.Write(writer, id);
        Thoth.Write(writer, currentDate);
        Thoth.Write(writer, rank);
        Thoth.Write(writer, xp);
        Thoth.Write(writer, units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            ((IThoth)units[i]).Write(writer);
        }
        if (version > 10)
        {
            Thoth.Write(writer, oldUnits.Count);
            for (int j = 0; j < oldUnits.Count; j++)
            {
                ((IThoth)oldUnits[j]).Write(writer);
            }
        }
        Thoth.Write(writer, items.Count);
        for (int k = 0; k < items.Count; k++)
        {
            ((IThoth)items[k]).Write(writer);
        }
        Thoth.Write(writer, marketEventId);
        Thoth.Write(writer, marketItems.Count);
        for (int l = 0; l < marketItems.Count; l++)
        {
            ((IThoth)marketItems[l]).Write(writer);
        }
        Thoth.Write(writer, addedMarketItems.Count);
        for (int m = 0; m < addedMarketItems.Count; m++)
        {
            ((IThoth)addedMarketItems[m]).Write(writer);
        }
        ((IThoth)stats).Write(writer);
        Thoth.Write(writer, factions.Count);
        for (int n = 0; n < factions.Count; n++)
        {
            ((IThoth)factions[n]).Write(writer);
        }
        Thoth.Write(writer, lateShipmentCount);
        Thoth.Write(writer, lastShipmentFailed);
        Thoth.Write(writer, nextShipmentExtraDays);
        Thoth.Write(writer, curCampaignIdx);
        Thoth.Write(writer, missions.Count);
        for (int num2 = 0; num2 < missions.Count; num2++)
        {
            ((IThoth)missions[num2]).Write(writer);
        }
        Thoth.Write(writer, scoutsSent);
        Thoth.Write(writer, skills.Count);
        for (int num3 = 0; num3 < skills.Count; num3++)
        {
            Thoth.Write(writer, (int)skills[num3]);
        }
        Thoth.Write(writer, outsiders.Count);
        for (int num4 = 0; num4 < outsiders.Count; num4++)
        {
            ((IThoth)outsiders[num4]).Write(writer);
        }
        Thoth.Write(writer, hideoutTutos);
        Thoth.Write(writer, smugglersMaxRankShown);
        Thoth.Write(writer, winningStreak);
        Thoth.Write(writer, warbandFaced);
        Thoth.Write(writer, inMission);
        if (inMission)
        {
            ((IThoth)endMission).Write(writer);
        }
        Thoth.Write(writer, lastMissionAmbushed);
        Thoth.Write(writer, overrideName);
        Thoth.Write(writer, availaibleRespec);
    }

    void IThoth.Read(BinaryReader reader)
    {
        int i = 0;
        Thoth.Read(reader, out int i2);
        lastVersion = i2;
        if (i2 > 26)
        {
            Thoth.Read(reader, out i);
        }
        Thoth.Read(reader, out name);
        Thoth.Read(reader, out id);
        if (i2 < 10)
        {
            Thoth.Read(reader, out campaignId);
        }
        Thoth.Read(reader, out currentDate);
        Thoth.Read(reader, out rank);
        Thoth.Read(reader, out xp);
        int i3 = 0;
        Thoth.Read(reader, out i3);
        for (int j = 0; j < i3; j++)
        {
            UnitSave unitSave = new UnitSave(UnitId.NONE);
            ((IThoth)unitSave).Read(reader);
            units.Add(unitSave);
        }
        if (i2 > 10)
        {
            Thoth.Read(reader, out i3);
            for (int k = 0; k < i3; k++)
            {
                UnitStatSave unitStatSave = new UnitStatSave();
                ((IThoth)unitStatSave).Read(reader);
                oldUnits.Add(unitStatSave);
            }
        }
        Thoth.Read(reader, out i3);
        for (int l = 0; l < i3; l++)
        {
            ItemSave itemSave = new ItemSave(ItemId.NONE);
            ((IThoth)itemSave).Read(reader);
            items.Add(itemSave);
        }
        if (i2 >= 14)
        {
            Thoth.Read(reader, out marketEventId);
            Thoth.Read(reader, out i3);
            for (int m = 0; m < i3; m++)
            {
                ItemSave itemSave2 = new ItemSave(ItemId.NONE);
                ((IThoth)itemSave2).Read(reader);
                marketItems.Add(itemSave2);
            }
        }
        if (i2 >= 15)
        {
            Thoth.Read(reader, out i3);
            for (int n = 0; n < i3; n++)
            {
                ItemSave itemSave3 = new ItemSave(ItemId.NONE);
                ((IThoth)itemSave3).Read(reader);
                addedMarketItems.Add(itemSave3);
            }
        }
        ((IThoth)stats).Read(reader);
        if (i2 > 8)
        {
            Thoth.Read(reader, out i3);
            for (int num = 0; num < i3; num++)
            {
                FactionSave factionSave = new FactionSave();
                ((IThoth)factionSave).Read(reader);
                factions.Add(factionSave);
            }
            Thoth.Read(reader, out lateShipmentCount);
            Thoth.Read(reader, out lastShipmentFailed);
            Thoth.Read(reader, out nextShipmentExtraDays);
        }
        if (i2 > 9)
        {
            Thoth.Read(reader, out curCampaignIdx);
            Thoth.Read(reader, out i3);
            for (int num2 = 0; num2 < i3; num2++)
            {
                MissionSave missionSave = new MissionSave(Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE));
                ((IThoth)missionSave).Read(reader);
                missions.Add(missionSave);
            }
        }
        if (i2 > 10)
        {
            Thoth.Read(reader, out scoutsSent);
        }
        if (i2 > 11)
        {
            Thoth.Read(reader, out i3);
            for (int num3 = 0; num3 < i3; num3++)
            {
                Thoth.Read(reader, out int i4);
                skills.Add((WarbandSkillId)i4);
            }
        }
        if (i2 > 18)
        {
            Thoth.Read(reader, out i3);
            for (int num4 = 0; num4 < i3; num4++)
            {
                UnitSave unitSave2 = new UnitSave(UnitId.NONE);
                ((IThoth)unitSave2).Read(reader);
                outsiders.Add(unitSave2);
            }
        }
        if (i2 > 20)
        {
            Thoth.Read(reader, out hideoutTutos);
        }
        if (i2 > 22)
        {
            Thoth.Read(reader, out smugglersMaxRankShown);
        }
        if (i2 > 21)
        {
            Thoth.Read(reader, out winningStreak);
        }
        if (i2 > 25)
        {
            Thoth.Read(reader, out warbandFaced);
        }
        if (i2 > 27)
        {
            Thoth.Read(reader, out inMission);
        }
        if (i2 > 28 && inMission)
        {
            ((IThoth)endMission).Read(reader);
        }
        if (i2 > 29)
        {
            Thoth.Read(reader, out lastMissionAmbushed);
        }
        if (i2 > 32)
        {
            Thoth.Read(reader, out overrideName);
        }
        if (i2 > 33)
        {
            Thoth.Read(reader, out availaibleRespec);
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
        num2 += currentDate;
        for (int i = 0; i < units.Count; i++)
        {
            num2 += units[i].GetCRC(read);
        }
        for (int j = 0; j < outsiders.Count; j++)
        {
            num2 += outsiders[j].GetCRC(read);
        }
        for (int k = 0; k < skills.Count; k++)
        {
            num2 = (int)(num2 + skills[k]);
        }
        for (int l = 0; l < oldUnits.Count; l++)
        {
            num2 += oldUnits[l].GetCRC(read);
        }
        for (int m = 0; m < items.Count; m++)
        {
            num2 += items[m].GetCRC(read);
        }
        for (int n = 0; n < marketItems.Count; n++)
        {
            num2 += marketItems[n].GetCRC(read);
        }
        for (int num3 = 0; num3 < addedMarketItems.Count; num3++)
        {
            num2 += addedMarketItems[num3].GetCRC(read);
        }
        num2 += stats.GetCRC(read);
        for (int num4 = 0; num4 < factions.Count; num4++)
        {
            num2 += factions[num4].GetCRC(read);
        }
        for (int num5 = 0; num5 < missions.Count; num5++)
        {
            num2 += missions[num5].GetCRC(read);
        }
        num2 += lateShipmentCount;
        num2 += (lastShipmentFailed ? 1 : 0);
        num2 += nextShipmentExtraDays;
        num2 += campaignId;
        num2 += curCampaignIdx;
        num2 += scoutsSent;
        num2 += marketEventId;
        num2 += (int)hideoutTutos;
        num2 += (smugglersMaxRankShown ? 1 : 0);
        for (int num6 = 0; num6 < unitsSlotsIndex.Count; num6++)
        {
            num2 += unitsSlotsIndex[num6];
        }
        for (int num7 = 0; num7 < exhibitionUnitsSlotsIndex.Count; num7++)
        {
            num2 += exhibitionUnitsSlotsIndex[num7];
        }
        num2 += winningStreak;
        if (num > 25)
        {
            num2 += warbandFaced;
        }
        if (num > 27)
        {
            num2 += (inMission ? 1 : 0);
        }
        if (inMission && num > 28)
        {
            num2 += endMission.GetCRC(read);
        }
        if (num > 29)
        {
            num2 += (lastMissionAmbushed ? 1 : 0);
        }
        if (num > 32)
        {
            num2 += (lastMissionAmbushed ? 1 : 0);
        }
        if (num > 33)
        {
            num2 += availaibleRespec;
        }
        return num2;
    }
}
