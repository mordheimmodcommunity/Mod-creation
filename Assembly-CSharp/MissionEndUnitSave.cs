using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MissionEndUnitSave : IThoth
{
    private int lastVersion;

    public UnitStateId status;

    public List<Item> items;

    public List<EndUnitEnchantment> enchantments;

    public UnitSave unitSave;

    public Vector3 position;

    public Vector3 rotation;

    public uint myrtilusId;

    public int currentMvu;

    public int[] mvuPerCategories;

    public int currentWounds;

    public int currentSP;

    public int currentOP;

    public UnitSlotId weaponSet;

    public bool turnStarted;

    public bool isPlayed;

    public int costOfLosingId;

    public List<InjuryData> injuries;

    public bool dead;

    public bool isLadderVisible;

    public bool isMaxRank;

    public List<KeyValuePair<int, string>> XPs;

    public List<UnitJoinUnitRankData> advancements;

    public List<Mutation> mutations;

    public List<Item> lostItems;

    public MissionEndUnitSave()
    {
        items = new List<Item>();
        injuries = new List<InjuryData>();
        XPs = new List<KeyValuePair<int, string>>();
        advancements = new List<UnitJoinUnitRankData>();
        mutations = new List<Mutation>();
        enchantments = new List<EndUnitEnchantment>();
        unitSave = new UnitSave();
        lostItems = new List<Item>();
        mvuPerCategories = new int[5];
    }

    void IThoth.Read(BinaryReader reader)
    {
        int i = 0;
        Thoth.Read(reader, out int i2);
        lastVersion = i2;
        Thoth.Read(reader, out i);
        int i3 = 0;
        Thoth.Read(reader, out i3);
        status = (UnitStateId)i3;
        Thoth.Read(reader, out costOfLosingId);
        int i4 = 0;
        Thoth.Read(reader, out i4);
        for (int j = 0; j < i4; j++)
        {
            EndUnitEnchantment item = default(EndUnitEnchantment);
            if (i2 == 0)
            {
                int i5 = 0;
                Thoth.Read(reader, out i5);
                item.enchantId = (EnchantmentId)i5;
            }
            else
            {
                Thoth.Read(reader, out item.guid);
                int i6 = 0;
                Thoth.Read(reader, out i6);
                item.enchantId = (EnchantmentId)i6;
                Thoth.Read(reader, out item.durationLeft);
                Thoth.Read(reader, out item.ownerMyrtilusId);
                if (i2 > 2)
                {
                    Thoth.Read(reader, out item.runeAllegianceId);
                }
            }
            enchantments.Add(item);
        }
        ((IThoth)unitSave).Read(reader);
        if (i2 > 0)
        {
            Thoth.Read(reader, out position.x);
            Thoth.Read(reader, out position.y);
            Thoth.Read(reader, out position.z);
            Thoth.Read(reader, out rotation.x);
            Thoth.Read(reader, out rotation.y);
            Thoth.Read(reader, out rotation.z);
            Thoth.Read(reader, out myrtilusId);
            Thoth.Read(reader, out currentWounds);
            Thoth.Read(reader, out currentSP);
            Thoth.Read(reader, out currentOP);
            int i7 = 0;
            Thoth.Read(reader, out i7);
            weaponSet = (UnitSlotId)i7;
            Thoth.Read(reader, out turnStarted);
        }
        if (i2 > 1)
        {
            Thoth.Read(reader, out isPlayed);
        }
        if (i2 > 3)
        {
            Thoth.Read(reader, out isLadderVisible);
        }
        else
        {
            isLadderVisible = (status != UnitStateId.OUT_OF_ACTION);
        }
        if (i2 > 4)
        {
            Thoth.Read(reader, out currentMvu);
            Thoth.Read(reader, out i4);
            if (i4 != 5)
            {
                mvuPerCategories = new int[5];
            }
            for (int k = 0; k < i4; k++)
            {
                Thoth.Read(reader, out int i8);
                mvuPerCategories[k] = i8;
            }
        }
        for (int l = 0; l < unitSave.items.Count; l++)
        {
            if (unitSave.items[l] == null)
            {
                items.Add(new Item(ItemId.NONE));
            }
            else
            {
                items.Add(new Item(unitSave.items[l]));
            }
        }
    }

    public int GetVersion()
    {
        return 5;
    }

    public int GetCRC(bool read)
    {
        return CalculateCRC(read);
    }

    public void UpdateUnit(UnitController unit)
    {
        myrtilusId = unit.uid;
        position = unit.transform.position;
        rotation = unit.transform.rotation.eulerAngles;
        currentWounds = unit.unit.CurrentWound;
        currentSP = unit.unit.CurrentStrategyPoints;
        currentOP = unit.unit.CurrentOffensePoints;
        currentMvu = unit.unit.GetAttribute(AttributeId.CURRENT_MVU);
        mvuPerCategories = unit.MVUptsPerCategory;
        unitSave = unit.unit.UnitSave;
        status = unit.unit.Status;
        items.Clear();
        items.AddRange(unit.unit.Items);
        isPlayed = (unit.IsPlayed() && unit.unit.CampaignData == null);
        isLadderVisible = unit.ladderVisible;
        weaponSet = unit.unit.ActiveWeaponSlot;
        turnStarted = unit.TurnStarted;
        enchantments.Clear();
        for (int i = 0; i < unit.unit.Enchantments.Count; i++)
        {
            if (!unit.unit.Enchantments[i].original)
            {
                EndUnitEnchantment item = default(EndUnitEnchantment);
                item.guid = unit.unit.Enchantments[i].guid;
                item.enchantId = unit.unit.Enchantments[i].Id;
                item.durationLeft = unit.unit.Enchantments[i].Duration;
                item.runeAllegianceId = (int)unit.unit.Enchantments[i].AllegianceId;
                UnitController unitController = PandoraSingleton<MissionManager>.Instance.GetUnitController(unit.unit.Enchantments[i].Provider, includeExclude: true);
                if (unitController != null)
                {
                    item.ownerMyrtilusId = unitController.uid;
                }
                else
                {
                    item.ownerMyrtilusId = unit.uid;
                }
                enchantments.Add(item);
            }
        }
    }

    public int GetAttribute(AttributeId attributeId)
    {
        return unitSave.stats.stats[(int)attributeId];
    }

    private int CalculateCRC(bool read)
    {
        int num = (!read) ? ((IThoth)this).GetVersion() : lastVersion;
        int num2 = 0;
        num2 = (int)(num2 + status);
        num2 += costOfLosingId;
        for (int i = 0; i < enchantments.Count; i++)
        {
            if (num > 0)
            {
                int num3 = num2;
                EndUnitEnchantment endUnitEnchantment = enchantments[i];
                num2 = num3 + (int)endUnitEnchantment.guid;
            }
            int num4 = num2;
            EndUnitEnchantment endUnitEnchantment2 = enchantments[i];
            num2 = (int)(num4 + endUnitEnchantment2.enchantId);
            if (num > 0)
            {
                int num5 = num2;
                EndUnitEnchantment endUnitEnchantment3 = enchantments[i];
                num2 = num5 + endUnitEnchantment3.durationLeft;
                int num6 = num2;
                EndUnitEnchantment endUnitEnchantment4 = enchantments[i];
                num2 = num6 + (int)endUnitEnchantment4.ownerMyrtilusId;
            }
            if (num > 2)
            {
                int num7 = num2;
                EndUnitEnchantment endUnitEnchantment5 = enchantments[i];
                num2 = num7 + endUnitEnchantment5.runeAllegianceId;
            }
        }
        num2 += unitSave.GetCRC(read);
        if (num > 0)
        {
            num2 += (int)position.x;
            num2 += (int)position.y;
            num2 += (int)position.z;
            num2 += (int)rotation.x;
            num2 += (int)rotation.y;
            num2 += (int)rotation.z;
            num2 += (int)myrtilusId;
            num2 += currentWounds;
            num2 += currentSP;
            num2 += currentOP;
            num2 = (int)(num2 + weaponSet);
            num2 += (turnStarted ? 1 : 0);
        }
        if (num > 1)
        {
            num2 += (isPlayed ? 1 : 0);
        }
        if (num > 3)
        {
            num2 += (isLadderVisible ? 1 : 0);
        }
        return num2;
    }

    public void Write(BinaryWriter writer)
    {
        int version = ((IThoth)this).GetVersion();
        Thoth.Write(writer, version);
        int cRC = GetCRC(read: false);
        Thoth.Write(writer, cRC);
        Thoth.Write(writer, (int)status);
        Thoth.Write(writer, costOfLosingId);
        Thoth.Write(writer, enchantments.Count);
        for (int i = 0; i < enchantments.Count; i++)
        {
            EndUnitEnchantment endUnitEnchantment = enchantments[i];
            Thoth.Write(writer, endUnitEnchantment.guid);
            EndUnitEnchantment endUnitEnchantment2 = enchantments[i];
            Thoth.Write(writer, (int)endUnitEnchantment2.enchantId);
            EndUnitEnchantment endUnitEnchantment3 = enchantments[i];
            Thoth.Write(writer, endUnitEnchantment3.durationLeft);
            EndUnitEnchantment endUnitEnchantment4 = enchantments[i];
            Thoth.Write(writer, endUnitEnchantment4.ownerMyrtilusId);
            EndUnitEnchantment endUnitEnchantment5 = enchantments[i];
            Thoth.Write(writer, endUnitEnchantment5.runeAllegianceId);
        }
        ((IThoth)unitSave).Write(writer);
        if (version > 0)
        {
            Thoth.Write(writer, position.x);
            Thoth.Write(writer, position.y);
            Thoth.Write(writer, position.z);
            Thoth.Write(writer, rotation.x);
            Thoth.Write(writer, rotation.y);
            Thoth.Write(writer, rotation.z);
            Thoth.Write(writer, myrtilusId);
            Thoth.Write(writer, currentWounds);
            Thoth.Write(writer, currentSP);
            Thoth.Write(writer, currentOP);
            Thoth.Write(writer, (int)weaponSet);
            Thoth.Write(writer, turnStarted);
        }
        if (version > 1)
        {
            Thoth.Write(writer, isPlayed);
        }
        if (version > 3)
        {
            Thoth.Write(writer, isLadderVisible);
        }
        if (version > 4)
        {
            Thoth.Write(writer, currentMvu);
            Thoth.Write(writer, mvuPerCategories.Length);
            for (int j = 0; j < mvuPerCategories.Length; j++)
            {
                Thoth.Write(writer, mvuPerCategories[j]);
            }
        }
    }
}
