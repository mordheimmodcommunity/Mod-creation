using System.Collections.Generic;
using System.IO;

public class UnitSave : IThoth
{
	private int lastVersion;

	public string bio;

	public int campaignId;

	public bool isOutsider;

	public bool isFreeOutsider;

	public Dictionary<AttributeId, int> attributes;

	public int xp;

	public UnitTypeId overrideTypeId;

	public UnitRankId rankId;

	public List<InjuryId> injuries;

	public List<ItemSave> items;

	public string skinColor;

	public Dictionary<BodyPartId, KeyValuePair<int, int>> customParts;

	public List<SkillId> spells;

	public List<SkillId> activeSkills;

	public List<SkillId> passiveSkills;

	public List<SkillId> consumableSkills;

	public List<MutationId> mutations;

	public int numLevelupUpdate;

	public int upkeepOwned;

	public int upkeepMissedDays;

	public int injuredTime;

	public int lastInjuryDate;

	public bool injuryPaid;

	public SkillId skillInTrainingId;

	public int trainingTime;

	public int warbandSlotIndex;

	public UnitStatSave stats;

	public bool isReinforcement;

	public bool mutationChecked;

	public UnitSave()
	{
		injuries = new List<InjuryId>();
		spells = new List<SkillId>();
		activeSkills = new List<SkillId>();
		passiveSkills = new List<SkillId>();
		consumableSkills = new List<SkillId>();
		mutations = new List<MutationId>();
		attributes = new Dictionary<AttributeId, int>(AttributeIdComparer.Instance);
		customParts = new Dictionary<BodyPartId, KeyValuePair<int, int>>(BodyPartIdComparer.Instance);
		items = new List<ItemSave>(13);
		for (int i = 0; i < 13; i++)
		{
			items.Add(null);
		}
		stats = new UnitStatSave();
		isReinforcement = false;
		mutationChecked = false;
	}

	public UnitSave(UnitId unitId)
		: this()
	{
		stats.id = (int)unitId;
		xp = 0;
		rankId = UnitRankId.UNIT_RANK_00;
		numLevelupUpdate = 0;
		upkeepOwned = 0;
		upkeepMissedDays = 0;
		injuredTime = 0;
		lastInjuryDate = 0;
		injuryPaid = true;
		skillInTrainingId = SkillId.NONE;
		skinColor = string.Empty;
		bio = string.Empty;
		trainingTime = 0;
		warbandSlotIndex = -1;
	}

	int IThoth.GetVersion()
	{
		return 19;
	}

	void IThoth.Write(BinaryWriter writer)
	{
		Thoth.Write(writer, ((IThoth)this).GetVersion());
		int cRC = GetCRC(read: false);
		Thoth.Write(writer, cRC);
		Thoth.Write(writer, campaignId);
		Thoth.Write(writer, bio);
		Thoth.Write(writer, isOutsider);
		Thoth.Write(writer, isFreeOutsider);
		Thoth.Write(writer, attributes.Count);
		foreach (KeyValuePair<AttributeId, int> attribute in attributes)
		{
			Thoth.Write(writer, (int)attribute.Key);
			Thoth.Write(writer, attribute.Value);
		}
		Thoth.Write(writer, xp);
		Thoth.Write(writer, (int)overrideTypeId);
		Thoth.Write(writer, (int)rankId);
		Thoth.Write(writer, numLevelupUpdate);
		Thoth.Write(writer, upkeepOwned);
		Thoth.Write(writer, upkeepMissedDays);
		Thoth.Write(writer, injuredTime);
		Thoth.Write(writer, lastInjuryDate);
		Thoth.Write(writer, injuryPaid);
		Thoth.Write(writer, (int)skillInTrainingId);
		Thoth.Write(writer, trainingTime);
		Thoth.Write(writer, warbandSlotIndex);
		Thoth.Write(writer, injuries.Count);
		for (int i = 0; i < injuries.Count; i++)
		{
			Thoth.Write(writer, (int)injuries[i]);
		}
		Thoth.Write(writer, items.Count);
		for (int j = 0; j < items.Count; j++)
		{
			if (items[j] != null)
			{
				Thoth.Write(writer, b: true);
				((IThoth)items[j]).Write(writer);
			}
			else
			{
				Thoth.Write(writer, b: false);
			}
		}
		Thoth.Write(writer, skinColor);
		Thoth.Write(writer, customParts.Count);
		foreach (KeyValuePair<BodyPartId, KeyValuePair<int, int>> customPart in customParts)
		{
			Thoth.Write(writer, (int)customPart.Key);
			Thoth.Write(writer, customPart.Value.Key);
			Thoth.Write(writer, customPart.Value.Value);
		}
		Thoth.Write(writer, spells.Count);
		for (int k = 0; k < spells.Count; k++)
		{
			Thoth.Write(writer, (int)spells[k]);
		}
		Thoth.Write(writer, activeSkills.Count);
		for (int l = 0; l < activeSkills.Count; l++)
		{
			Thoth.Write(writer, (int)activeSkills[l]);
		}
		Thoth.Write(writer, passiveSkills.Count);
		for (int m = 0; m < passiveSkills.Count; m++)
		{
			Thoth.Write(writer, (int)passiveSkills[m]);
		}
		Thoth.Write(writer, consumableSkills.Count);
		for (int n = 0; n < consumableSkills.Count; n++)
		{
			Thoth.Write(writer, (int)consumableSkills[n]);
		}
		Thoth.Write(writer, mutations.Count);
		for (int num = 0; num < mutations.Count; num++)
		{
			Thoth.Write(writer, (int)mutations[num]);
		}
		((IThoth)stats).Write(writer);
		Thoth.Write(writer, isReinforcement);
		Thoth.Write(writer, mutationChecked);
	}

	void IThoth.Read(BinaryReader reader)
	{
		int i = 0;
		Thoth.Read(reader, out int i2);
		lastVersion = i2;
		if (i2 >= 17)
		{
			Thoth.Read(reader, out i);
		}
		Thoth.Read(reader, out campaignId);
		if (i2 >= 12)
		{
			Thoth.Read(reader, out bio);
		}
		if (i2 >= 13)
		{
			Thoth.Read(reader, out isOutsider);
		}
		if (i2 >= 14)
		{
			Thoth.Read(reader, out isFreeOutsider);
		}
		int i3 = 0;
		Thoth.Read(reader, out i3);
		for (int j = 0; j < i3; j++)
		{
			Thoth.Read(reader, out int i4);
			Thoth.Read(reader, out int i5);
			attributes.Add((AttributeId)i4, i5);
		}
		Thoth.Read(reader, out xp);
		if (i2 >= 5)
		{
			int i6 = 0;
			Thoth.Read(reader, out i6);
			overrideTypeId = (UnitTypeId)i6;
		}
		else
		{
			overrideTypeId = UnitTypeId.NONE;
		}
		if (i2 >= 3)
		{
			int i7 = 0;
			Thoth.Read(reader, out i7);
			rankId = (UnitRankId)i7;
		}
		else
		{
			rankId = UnitRankId.UNIT_RANK_00;
		}
		Thoth.Read(reader, out numLevelupUpdate);
		Thoth.Read(reader, out upkeepOwned);
		Thoth.Read(reader, out upkeepMissedDays);
		Thoth.Read(reader, out injuredTime);
		Thoth.Read(reader, out lastInjuryDate);
		Thoth.Read(reader, out injuryPaid);
		int i8 = 0;
		Thoth.Read(reader, out i8);
		skillInTrainingId = (SkillId)i8;
		if (i2 <= 3)
		{
			Thoth.Read(reader, out i8);
		}
		Thoth.Read(reader, out trainingTime);
		if (i2 < 15)
		{
			Thoth.Read(reader, out int i9);
			Thoth.Read(reader, out i9);
		}
		if (i2 < 16)
		{
			Thoth.Read(reader, out int _);
		}
		if (i2 >= 2)
		{
			Thoth.Read(reader, out warbandSlotIndex);
		}
		Thoth.Read(reader, out i3);
		for (int k = 0; k < i3; k++)
		{
			int i11 = 0;
			Thoth.Read(reader, out i11);
			injuries.Add((InjuryId)i11);
		}
		Thoth.Read(reader, out i3);
		for (int l = 0; l < i3; l++)
		{
			bool b = false;
			Thoth.Read(reader, out b);
			if (b)
			{
				ItemSave itemSave = new ItemSave(ItemId.NONE);
				((IThoth)itemSave).Read(reader);
				items[l] = itemSave;
			}
		}
		Thoth.Read(reader, out skinColor);
		if (i2 >= 10 && i2 < 14 && i2 < 14)
		{
			Thoth.Read(reader, out int _);
		}
		if (i2 >= 11)
		{
			if (i2 >= 13)
			{
				Thoth.Read(reader, out i3);
				for (int m = 0; m < i3; m++)
				{
					Thoth.Read(reader, out int i13);
					Thoth.Read(reader, out int i14);
					Thoth.Read(reader, out int i15);
					customParts.Add((BodyPartId)i13, new KeyValuePair<int, int>(i14, i15));
				}
			}
			else
			{
				Thoth.Read(reader, out i3);
				for (int n = 0; n < i3; n++)
				{
					Thoth.Read(reader, out int i16);
					Thoth.Read(reader, out string _);
					customParts.Add((BodyPartId)i16, new KeyValuePair<int, int>(1, 1));
				}
			}
		}
		Thoth.Read(reader, out i3);
		for (int num = 0; num < i3; num++)
		{
			Thoth.Read(reader, out int i17);
			spells.Add((SkillId)i17);
		}
		Thoth.Read(reader, out i3);
		for (int num2 = 0; num2 < i3; num2++)
		{
			Thoth.Read(reader, out int i18);
			activeSkills.Add((SkillId)i18);
		}
		Thoth.Read(reader, out i3);
		for (int num3 = 0; num3 < i3; num3++)
		{
			Thoth.Read(reader, out int i19);
			passiveSkills.Add((SkillId)i19);
		}
		if (i2 >= 8)
		{
			Thoth.Read(reader, out i3);
			for (int num4 = 0; num4 < i3; num4++)
			{
				Thoth.Read(reader, out int i20);
				consumableSkills.Add((SkillId)i20);
			}
		}
		Thoth.Read(reader, out i3);
		for (int num5 = 0; num5 < i3; num5++)
		{
			Thoth.Read(reader, out int i21);
			mutations.Add((MutationId)i21);
		}
		if (i2 > 1)
		{
			((IThoth)stats).Read(reader);
		}
		if (i2 > 17)
		{
			Thoth.Read(reader, out isReinforcement);
		}
		if (i2 > 18)
		{
			Thoth.Read(reader, out mutationChecked);
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
		char[] array = bio.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			num2 += array[i];
		}
		num2 += campaignId;
		num2 += (isOutsider ? 1 : 0);
		num2 += (isFreeOutsider ? 1 : 0);
		foreach (KeyValuePair<AttributeId, int> attribute in attributes)
		{
			num2 = (int)(num2 + (attribute.Value + attribute.Key));
		}
		num2 += xp;
		num2 = (int)(num2 + overrideTypeId);
		num2 = (int)(num2 + rankId);
		for (int j = 0; j < injuries.Count; j++)
		{
			num2 = (int)(num2 + injuries[j]);
		}
		for (int k = 0; k < items.Count; k++)
		{
			if (items[k] != null)
			{
				num2 += items[k].GetCRC(read);
			}
		}
		array = skinColor.ToCharArray();
		for (int l = 0; l < array.Length; l++)
		{
			num2 += array[l];
		}
		foreach (KeyValuePair<BodyPartId, KeyValuePair<int, int>> customPart in customParts)
		{
			num2 = (int)(num2 + (customPart.Value.Key + customPart.Value.Value + customPart.Key));
		}
		for (int m = 0; m < spells.Count; m++)
		{
			num2 = (int)(num2 + spells[m]);
		}
		for (int n = 0; n < activeSkills.Count; n++)
		{
			num2 = (int)(num2 + activeSkills[n]);
		}
		for (int num3 = 0; num3 < passiveSkills.Count; num3++)
		{
			num2 = (int)(num2 + passiveSkills[num3]);
		}
		for (int num4 = 0; num4 < consumableSkills.Count; num4++)
		{
			num2 = (int)(num2 + consumableSkills[num4]);
		}
		for (int num5 = 0; num5 < mutations.Count; num5++)
		{
			num2 = (int)(num2 + mutations[num5]);
		}
		num2 += numLevelupUpdate;
		num2 += upkeepOwned;
		num2 += upkeepMissedDays;
		num2 += injuredTime;
		num2 += lastInjuryDate;
		num2 += (injuryPaid ? 1 : 0);
		num2 = (int)(num2 + skillInTrainingId);
		num2 += trainingTime;
		num2 += warbandSlotIndex;
		num2 += stats.GetCRC(read);
		if (num > 17)
		{
			num2 += (isReinforcement ? 1 : 0);
		}
		if (num > 18)
		{
			num2 += (mutationChecked ? 1 : 0);
		}
		return num2;
	}
}
