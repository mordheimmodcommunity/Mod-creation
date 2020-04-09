public abstract class Achievement
{
	private string locName;

	private string locDesc;

	public AchievementData Data
	{
		get;
		private set;
	}

	public AchievementId Id => Data.Id;

	public bool Completed
	{
		get;
		set;
	}

	public AchievementTargetId Target => Data.AchievementTargetId;

	public int Xp => Data.Xp;

	public string LocName
	{
		get
		{
			if (locName == null)
			{
				locName = PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_title_" + Data.Name);
			}
			return locName;
		}
	}

	public string LocDesc
	{
		get
		{
			if (locDesc == null)
			{
				locDesc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_desc_" + Data.Name);
			}
			return locDesc;
		}
	}

	protected Achievement(AchievementData data)
	{
		Data = data;
	}

	public static Achievement Create(AchievementData data)
	{
		switch (data.AchievementTypeId)
		{
		case AchievementTypeId.STAT:
			return new AchievementStat(data);
		case AchievementTypeId.CAMPAIGN_MISSION:
			return new AchievementCampaignMission(data);
		case AchievementTypeId.EQUIP_ITEM_QUALITY:
		case AchievementTypeId.EQUIP_RUNE_QUALITY:
			return new AchievementEquipItem(data);
		default:
			if (data.Id != 0)
			{
			}
			return null;
		}
	}

	public virtual bool CheckProfile(WarbandAttributeId statId, int value)
	{
		return false;
	}

	public virtual bool CheckWarband(Warband warband, WarbandAttributeId statId, int value)
	{
		return false;
	}

	public virtual bool CheckUnit(Unit unit, AttributeId statId, int value)
	{
		return false;
	}

	public virtual bool CheckEquipUnit(Unit unit, UnitSlotId slotId)
	{
		return false;
	}

	public virtual bool CheckFinishMission(CampaignMissionId missionId)
	{
		return false;
	}

	public bool CanCheck()
	{
		return !Completed && (Data.AchievementIdRequire == AchievementId.NONE || PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(Data.AchievementIdRequire));
	}

	public int Unlock()
	{
		Completed = true;
		return Data.Xp;
	}

	protected bool IsOfUnitType(Unit unit, UnitTypeId requiredUnitTypeId)
	{
		UnitTypeId unitTypeId = unit.GetUnitTypeId();
		return requiredUnitTypeId == UnitTypeId.NONE || requiredUnitTypeId == unitTypeId || (requiredUnitTypeId == UnitTypeId.HERO_1 && (unitTypeId == UnitTypeId.HERO_2 || unitTypeId == UnitTypeId.HERO_3)) || (requiredUnitTypeId == UnitTypeId.ACHIEVEMENT_OUTSIDER && unit.UnitSave.isOutsider);
	}
}
