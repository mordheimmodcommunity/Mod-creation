using System.Collections.Generic;

public class AchievementEquipItem : Achievement
{
	private readonly List<AchievementEquipItemQualityData> achievementEquipItemQualityDatas;

	public AchievementEquipItem(AchievementData data)
		: base(data)
	{
		achievementEquipItemQualityDatas = PandoraSingleton<DataFactory>.Instance.InitData<AchievementEquipItemQualityData>("fk_achievement_id", data.Id.ToIntString());
	}

	public override bool CheckEquipUnit(Unit unit, UnitSlotId slotId)
	{
		if (slotId < UnitSlotId.ITEM_1)
		{
			for (int i = 0; i < achievementEquipItemQualityDatas.Count; i++)
			{
				if (!IsOfUnitType(unit, achievementEquipItemQualityDatas[i].UnitTypeId))
				{
					continue;
				}
				bool flag = true;
				for (UnitSlotId unitSlotId = UnitSlotId.HELMET; unitSlotId < UnitSlotId.ITEM_1 && flag; unitSlotId++)
				{
					if (unitSlotId == UnitSlotId.SET1_OFFHAND || unitSlotId == UnitSlotId.SET2_OFFHAND)
					{
						Item item = unit.Items[(int)(unitSlotId - 1)];
						if (item.IsPaired || item.IsTwoHanded)
						{
							continue;
						}
					}
					Item item2 = unit.Items[(int)unitSlotId];
					if (achievementEquipItemQualityDatas[i].ItemQualityId != 0)
					{
						if (item2 == null || item2.Id == ItemId.NONE || achievementEquipItemQualityDatas[i].ItemQualityId != item2.QualityData.Id)
						{
							flag = false;
						}
					}
					else if (achievementEquipItemQualityDatas[i].RuneMarkQualityId != 0)
					{
						if (item2 == null || item2.Id == ItemId.NONE || item2.RuneMark == null || achievementEquipItemQualityDatas[i].RuneMarkQualityId != item2.RuneMark.QualityData.Id)
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}
}
