using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarbandMenuController
{
	public GameObject banner;

	public GameObject wagon;

	public GameObject idol;

	public HideoutMissionMap map;

	public GameObject mapPawn;

	public UnitMenuController dramatis;

	public List<UnitMenuController> unitCtrlrs;

	public List<UnitMenuController> hireableUnits;

	public List<FactionMenuController> factionCtrlrs;

	public int teamIndex;

	public bool generatingHireable;

	private int asyncQueue;

	public Warband Warband
	{
		get;
		private set;
	}

	public FactionMenuController PrimaryFactionController
	{
		get;
		private set;
	}

	public List<Unit> Units => Warband.Units;

	public WarbandMenuController(WarbandSave save)
	{
		Warband = new Warband(save);
		unitCtrlrs = new List<UnitMenuController>();
		InitFactions();
		hireableUnits = new List<UnitMenuController>();
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.PROFILE_RANK_UP, OnProfileRankUp);
		asyncQueue = 0;
	}

	private void OnProfileRankUp()
	{
		Warband.RefreshPlayerSkills(isNew: true);
	}

	private void InitFactions()
	{
		factionCtrlrs = new List<FactionMenuController>();
		for (int i = 0; i < Warband.Factions.Count; i++)
		{
			FactionMenuController factionMenuController = new FactionMenuController(Warband.Factions[i], Warband.GetWarbandSave());
			factionCtrlrs.Add(factionMenuController);
			if (Warband.Factions[i].Primary)
			{
				PrimaryFactionController = factionMenuController;
			}
		}
	}

	public void RefreshFactions()
	{
		for (int i = 0; i < factionCtrlrs.Count; i++)
		{
			factionCtrlrs[i].Refresh();
		}
	}

	private int UnitHireOrderCompare(UnitMenuController a, UnitMenuController b)
	{
		int num = b.unit.GetHireCost() - a.unit.GetHireCost();
		if (num == 0)
		{
			num = b.unit.Id - a.unit.Id;
		}
		return num;
	}

	public List<UnitMenuController> GetHireableUnits(int slotIndex, bool isImpressive)
	{
		List<UnitMenuController> list = new List<UnitMenuController>();
		for (int i = 0; i < hireableUnits.Count; i++)
		{
			if (Warband.IsActiveWarbandSlot(slotIndex) && Warband.IsUnitCountExceeded(hireableUnits[i].unit))
			{
				continue;
			}
			if (isImpressive)
			{
				if (hireableUnits[i].unit.GetUnitTypeId() == UnitTypeId.IMPRESSIVE)
				{
					list.Add(hireableUnits[i]);
				}
			}
			else if (hireableUnits[i].unit.GetUnitTypeId() != UnitTypeId.IMPRESSIVE && Warband.CanPlaceUnitAt(hireableUnits[i].unit, slotIndex) && (!Warband.IsActiveWarbandSlot(slotIndex) || !Warband.IsUnitCountExceeded(hireableUnits[i].unit, slotIndex)))
			{
				list.Add(hireableUnits[i]);
			}
		}
		list.Sort(UnitHireOrderCompare);
		return list;
	}

	public List<UnitMenuController> GetHiredSwordUnits()
	{
		List<UnitMenuController> list = new List<UnitMenuController>();
		for (int i = 0; i < hireableUnits.Count; i++)
		{
			if (hireableUnits[i].unit.Rank > 0)
			{
				list.Add(hireableUnits[i]);
			}
		}
		list.Sort(UnitHireOrderCompare);
		return list;
	}

	public void HireUnit(UnitMenuController currentUnit)
	{
		hireableUnits.Remove(currentUnit);
		unitCtrlrs.Add(currentUnit);
		if (currentUnit.unit.UnitSave.isOutsider)
		{
			Warband.HireOutsider(currentUnit.unit);
		}
		else
		{
			Warband.HireUnit(currentUnit.unit);
		}
		GenerateHireableUnits();
	}

	public void GenerateBanner()
	{
		asyncQueue++;
		string bannerName = Warband.GetBannerName();
		GenerateBanner_(bannerName, delegate(GameObject go)
		{
			banner = go;
		});
	}

	public void GenerateBanner_(string bannerName, Action<GameObject> cb)
	{
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/banners/", AssetBundleId.PROPS, bannerName + ".prefab", delegate(UnityEngine.Object go)
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)go);
			cb(obj);
			FinishedLoadingItem();
		});
	}

	public static void GenerateBanner(string bannerName, Action<GameObject> cb)
	{
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/banners/", AssetBundleId.PROPS, bannerName + ".prefab", delegate(UnityEngine.Object go)
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)go);
			cb(obj);
		});
	}

	public void GenerateIdol()
	{
		asyncQueue++;
		string text = Warband.GetIdolName() + "_menu";
		PandoraDebug.LogDebug("Instantiating Idol = " + text);
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/idols/", AssetBundleId.PROPS, text + ".prefab", delegate(UnityEngine.Object go)
		{
			idol = UnityEngine.Object.Instantiate((GameObject)go);
			FinishedLoadingItem();
		});
	}

	public void GenerateMap()
	{
		asyncQueue++;
		string text = Warband.GetMapName() + "_menu";
		PandoraDebug.LogDebug("Instantiating Map = " + text);
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/maps/", AssetBundleId.PROPS, text + ".prefab", delegate(UnityEngine.Object mapPrefab)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(mapPrefab);
			map = gameObject.GetComponent<HideoutMissionMap>();
			FinishedLoadingItem();
		});
		string str = Warband.GetMapPawnName() + "_menu";
		PandoraDebug.LogDebug("Instantiating Pawn = " + text);
		asyncQueue++;
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/maps/", AssetBundleId.PROPS, str + ".prefab", delegate(UnityEngine.Object go)
		{
			mapPawn = (GameObject)UnityEngine.Object.Instantiate(go);
			FinishedLoadingItem();
		});
	}

	public void Destroy()
	{
		for (int i = 0; i < unitCtrlrs.Count; i++)
		{
			UnityEngine.Object.Destroy(unitCtrlrs[i].gameObject);
		}
		unitCtrlrs.Clear();
	}

	public bool HasLeader(bool needToBeActive)
	{
		return Warband.HasLeader(needToBeActive);
	}

	public UnitMenuController GetLeader()
	{
		UnitMenuController result = null;
		for (int i = 0; i < unitCtrlrs.Count; i++)
		{
			if (unitCtrlrs[i].unit.UnitSave.warbandSlotIndex == 2)
			{
				if (unitCtrlrs[i].unit.GetActiveStatus() == UnitActiveStatusId.AVAILABLE)
				{
					return unitCtrlrs[i];
				}
				result = unitCtrlrs[i];
			}
			else if (unitCtrlrs[i].unit.IsLeader)
			{
				result = unitCtrlrs[i];
			}
		}
		return result;
	}

	public UnitMenuController GetDramatis()
	{
		return dramatis;
	}

	public void SetBannerWagon()
	{
		GenerateBanner();
		GenerateIdol();
		asyncQueue++;
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/", AssetBundleId.PROPS, Warband.WarbandData.Wagon + "_menu.prefab", delegate(UnityEngine.Object go)
		{
			wagon = UnityEngine.Object.Instantiate((GameObject)go);
			InteractivePoint[] componentsInChildren = wagon.GetComponentsInChildren<InteractivePoint>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i]);
			}
			componentsInChildren = null;
			FinishedLoadingItem();
		});
	}

	public List<Item> Disband(Unit unit, EventLogger.LogEvent reason, int data)
	{
		for (int i = 0; i < unitCtrlrs.Count; i++)
		{
			if (unitCtrlrs[i].unit == unit)
			{
				unitCtrlrs.RemoveAt(i);
				break;
			}
		}
		Debug.LogWarning("Disband: unitctrl does not exist for this unit");
		List<Item> list = Warband.Disband(unit);
		switch (reason)
		{
		case EventLogger.LogEvent.FIRE:
		case EventLogger.LogEvent.DEATH:
		case EventLogger.LogEvent.RETIREMENT:
			PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItems(list);
			break;
		}
		unit.Logger.AddHistory(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, reason, data);
		return list;
	}

	public void DisbandWarband()
	{
		while (unitCtrlrs.Count > 0)
		{
			Disband(unitCtrlrs[0].unit, EventLogger.LogEvent.FIRE, 0);
		}
	}

	public IEnumerator GenerateUnits()
	{
		Destroy();
		for (int i = 0; i < Units.Count; i++)
		{
			asyncQueue++;
			PandoraSingleton<GameManager>.Instance.StartCoroutine(UnitMenuController.LoadUnitPrefabAsync(Units[i], delegate(GameObject go)
			{
				UnitMenuController component = go.GetComponent<UnitMenuController>();
				unitCtrlrs.Add(component);
				component.transform.localPosition = new Vector3((float)unitCtrlrs.Count * 5f, 0f, 0f);
			}, FinishedLoadingItem));
			yield return null;
		}
		unitCtrlrs.Sort(new CompareUnitMenuController());
		asyncQueue++;
		PandoraSingleton<GameManager>.Instance.StartCoroutine(UnitMenuController.LoadUnitPrefabAsync(new Unit(new UnitSave(Warband.WarbandData.UnitIdDramatis)), delegate(GameObject go)
		{
			dramatis = go.GetComponent<UnitMenuController>();
		}, FinishedLoadingItem));
		yield return null;
		GenerateHireableUnits();
	}

	private void FinishedLoadingItem()
	{
		asyncQueue--;
		if (asyncQueue == 0 && !PandoraSingleton<HideoutManager>.Instance.startedLoading)
		{
			PandoraSingleton<AssetBundleLoader>.Instance.UnloadScenes();
			PandoraSingleton<HideoutManager>.Instance.finishedLoading = true;
		}
	}

	public void GenerateHireableUnits()
	{
		if (generatingHireable)
		{
			return;
		}
		generatingHireable = true;
		for (int num = hireableUnits.Count - 1; num >= 0; num--)
		{
			if (hireableUnits[num].unit.UnitSave.isOutsider && !Warband.Outsiders.Contains(hireableUnits[num].unit))
			{
				UnityEngine.Object.Destroy(hireableUnits[num].gameObject);
				hireableUnits.RemoveAt(num);
			}
		}
		for (int i = 0; i < Warband.HireableUnitIds.Count; i++)
		{
			UnitId unitId = Warband.HireableUnitIds[i];
			if (!IsUnitGenerated(unitId))
			{
				if ((unitId != UnitId.SMUGGLER || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.SMUGGLER)) && (unitId != UnitId.GLOBADIER || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.GLOBADIER)) && (unitId != UnitId.PRIEST_OF_ULRIC || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.PRIEST_OF_ULRIC)) && (unitId != UnitId.DOOMWEAVER || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.DOOMWEAVER)))
				{
					Unit unit = Unit.GenerateHireUnit(unitId, 0, 0);
					asyncQueue++;
					int index2 = i;
					PandoraSingleton<GameManager>.Instance.StartCoroutine(UnitMenuController.LoadUnitPrefabAsync(unit, delegate(GameObject go)
					{
						UnitMenuController component2 = go.GetComponent<UnitMenuController>();
						hireableUnits.Add(component2);
						component2.transform.localPosition = new Vector3((float)index2 * 5f, 0f, 0f);
					}, FinishedLoadingItem));
				}
			}
		}
		for (int j = 0; j < Warband.Outsiders.Count; j++)
		{
			if (!IsOutsiderGenerated(Warband.Outsiders[j]))
			{
				if ((Warband.Outsiders[j].Id != UnitId.SMUGGLER || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.SMUGGLER)) && (Warband.Outsiders[j].Id != UnitId.GLOBADIER || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.GLOBADIER)) && (Warband.Outsiders[j].Id != UnitId.PRIEST_OF_ULRIC || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.PRIEST_OF_ULRIC)) && (Warband.Outsiders[j].Id != UnitId.DOOMWEAVER || PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.DOOMWEAVER)))
				{
					asyncQueue++;
					int index = j;
					PandoraSingleton<GameManager>.Instance.StartCoroutine(UnitMenuController.LoadUnitPrefabAsync(Warband.Outsiders[index], delegate(GameObject go)
					{
						UnitMenuController component = go.GetComponent<UnitMenuController>();
						hireableUnits.Add(component);
						component.transform.localPosition = new Vector3((float)index * 5f, 0f, 10f);
					}, FinishedLoadingItem));
				}
			}
		}
	}

	public bool IsUnitGenerated(UnitId unitId)
	{
		for (int i = 0; i < hireableUnits.Count; i++)
		{
			if (!hireableUnits[i].unit.UnitSave.isOutsider && hireableUnits[i].unit.Id == unitId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOutsiderGenerated(Unit unit)
	{
		for (int i = 0; i < hireableUnits.Count; i++)
		{
			if (hireableUnits[i].unit.UnitSave.isOutsider && hireableUnits[i].unit == unit)
			{
				return true;
			}
		}
		return false;
	}

	public Unit GetLeaderUnit()
	{
		for (int i = 0; i < Units.Count; i++)
		{
			if (Units[i].IsActiveLeader)
			{
				return Units[i];
			}
		}
		return null;
	}

	public List<Unit> GetUnitsStatus(UnitActiveStatusId status)
	{
		List<Unit> list = new List<Unit>();
		for (int i = 0; i < Units.Count; i++)
		{
			if (Units[i].GetActiveStatus() == status)
			{
				list.Add(Units[i]);
			}
		}
		return list;
	}

	public List<Unit> GetActiveUnits()
	{
		List<Unit> list = new List<Unit>();
		for (int i = 0; i < Units.Count; i++)
		{
			if (Units[i].Active)
			{
				list.Add(Units[i]);
			}
		}
		return list;
	}

	public int GetActiveUnitsCount()
	{
		return Warband.GetUnitsCount(needToBeActive: true);
	}

	public List<UnitSave> GetActiveUnitsSave()
	{
		List<UnitSave> list = new List<UnitSave>();
		for (int i = 0; i < Units.Count; i++)
		{
			if (Units[i].Active)
			{
				list.Add(Units[i].UnitSave);
			}
		}
		return list;
	}

	public string[] GetActiveUnitsSerialized()
	{
		List<UnitSave> activeUnitsSave = GetActiveUnitsSave();
		List<string> list = new List<string>();
		for (int i = 0; i < activeUnitsSave.Count; i++)
		{
			list.Add(Thoth.WriteToString(activeUnitsSave[i]));
		}
		return list.ToArray();
	}

	public int GetSkirmishRating(List<int> unitsPosition)
	{
		int num = 0;
		if (unitsPosition != null)
		{
			for (int i = 0; i < unitsPosition.Count; i++)
			{
				if (Warband.IsActiveWarbandSlot(unitsPosition[i]))
				{
					num += Units[i].GetRating();
				}
			}
		}
		else
		{
			num = Warband.GetRating();
		}
		return num;
	}

	public void GetSkirmishInfo(List<int> unitsPosition, out int rating, out string[] serializedUnits)
	{
		if (unitsPosition != null)
		{
			rating = 0;
			List<string> list = new List<string>();
			for (int i = 0; i < unitsPosition.Count; i++)
			{
				if (Warband.IsActiveWarbandSlot(unitsPosition[i]))
				{
					rating += Units[i].GetRating();
					list.Add(Thoth.WriteToString(Units[i].UnitSave));
				}
			}
			serializedUnits = list.ToArray();
		}
		else
		{
			rating = Warband.GetRating();
			serializedUnits = GetActiveUnitsSerialized();
		}
	}

	public List<RuneMark> GetAvailableRuneMarks(UnitSlotId unitSlotId, Item item, out string reason, ref List<RuneMark> availableRuneMarks, ref List<RuneMark> notAvailableRuneMarks)
	{
		availableRuneMarks.Clear();
		notAvailableRuneMarks.Clear();
		List<RuneMark> result = new List<RuneMark>();
		reason = string.Empty;
		List<RuneMarkJoinItemTypeData> list = new List<RuneMarkJoinItemTypeData>();
		if (item.Id == ItemId.NONE)
		{
			if (unitSlotId == UnitSlotId.SET2_MAINHAND || unitSlotId == UnitSlotId.SET2_OFFHAND)
			{
				unitSlotId -= 2;
			}
			List<ItemJoinUnitSlotData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<ItemJoinUnitSlotData>("fk_unit_slot_id", unitSlotId.ToIntString());
			for (int j = 0; j < list2.Count; j++)
			{
				ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)list2[j].ItemId);
				bool flag = false;
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].ItemTypeId == itemData.ItemTypeId)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.AddRange(PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkJoinItemTypeData>("fk_item_type_id", itemData.ItemTypeId.ToIntString()));
				}
			}
		}
		else
		{
			list = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkJoinItemTypeData>("fk_item_type_id", item.TypeData.Id.ToIntString());
		}
		List<RuneMarkQualityData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkQualityData>();
		if (item.QualityData.RuneMarkQualityIdMax == RuneMarkQualityId.NONE)
		{
			reason = "na_enchant_no_slot";
		}
		else if (item.RuneMark != null && item.RuneMark.Data.Id != 0)
		{
			reason = "na_enchant_used_slot";
		}
		List<RuneMarkData> runeMarksData = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkData>("released", "1");
		for (int i = 0; i < runeMarksData.Count; i++)
		{
			if (runeMarksData[i].Id == RuneMarkId.NONE || !list.Exists((RuneMarkJoinItemTypeData x) => x.RuneMarkId == runeMarksData[i].Id))
			{
				continue;
			}
			for (int l = 0; l < list3.Count; l++)
			{
				if (list3[l].Id == RuneMarkQualityId.NONE)
				{
					continue;
				}
				string text = reason;
				List<RuneMarkRecipeData> list4 = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkRecipeData>("fk_rune_mark_id", ((int)runeMarksData[i].Id).ToConstantString(), "fk_rune_mark_quality_id", ((int)list3[l].Id).ToConstantString());
				if (list4 != null && list4.Count > 0)
				{
					if (!PandoraSingleton<HideoutManager>.Instance.WarbandChest.HasItem(list4[0].ItemId, ItemQualityId.NORMAL))
					{
						text = "na_enchant_not_found";
					}
					else if (list3[l].Id > item.QualityData.RuneMarkQualityIdMax)
					{
						text = "na_enchant_quality";
					}
					RuneMark item2 = new RuneMark(runeMarksData[i].Id, list3[l].Id, Warband.WarbandData.AllegianceId, item.TypeData.Id, text);
					if (string.IsNullOrEmpty(text))
					{
						availableRuneMarks.Add(item2);
					}
					else
					{
						notAvailableRuneMarks.Add(item2);
					}
				}
			}
		}
		return result;
	}

	public static int Compare(UnitMenuController x, UnitMenuController y)
	{
		if (x.unit.IsLeader && !y.unit.IsLeader)
		{
			return -1;
		}
		if (!x.unit.IsLeader && y.unit.IsLeader)
		{
			return 1;
		}
		if (x.unit.IsHero() && !y.unit.IsHero())
		{
			return -1;
		}
		if (!x.unit.IsHero() && y.unit.IsHero())
		{
			return 1;
		}
		if (x.unit.GetUnitTypeId() == UnitTypeId.HENCHMEN && y.unit.GetUnitTypeId() != UnitTypeId.HENCHMEN)
		{
			return -1;
		}
		if (x.unit.GetUnitTypeId() != UnitTypeId.HENCHMEN && y.unit.GetUnitTypeId() == UnitTypeId.HENCHMEN)
		{
			return 1;
		}
		if (x.unit.GetRating() > y.unit.GetRating())
		{
			return -1;
		}
		if (x.unit.GetRating() < y.unit.GetRating())
		{
			return 1;
		}
		if (x.unit.Xp > y.unit.Xp)
		{
			return -1;
		}
		if (x.unit.Xp < y.unit.Xp)
		{
			return 1;
		}
		return 0;
	}

	public UnitMenuController GetUnitByName(string unitName)
	{
		for (int i = 0; i < unitCtrlrs.Count; i++)
		{
			if (string.CompareOrdinal(unitCtrlrs[i].unit.Name, unitName) == 0)
			{
				return unitCtrlrs[i];
			}
		}
		return null;
	}

	public Unit GetLowestRankUnit(UnitTypeId unitTypeId, InjuryData consequenceDataInjuryId)
	{
		List<InjuryId> toExcludes = new List<InjuryId>();
		Unit unit = null;
		for (int i = 0; i < Units.Count; i++)
		{
			if (Units[i].GetUnitTypeId() == unitTypeId && Units[i].CanRollInjury(consequenceDataInjuryId, toExcludes, Units[i].GetInjuryModifiers()) && (unit == null || Units[i].Rank < unit.Rank || (Units[i].Rank == unit.Rank && Units[i].GetRating() < unit.GetRating())))
			{
				unit = Units[i];
			}
		}
		return unit;
	}

	public Unit GetHighestRankUnit(UnitTypeId unitTypeId, InjuryData consequenceDataInjuryId)
	{
		Unit unit = null;
		for (int i = 0; i < Units.Count; i++)
		{
			if (Units[i].GetUnitTypeId() == unitTypeId && (unit == null || Units[i].Rank > unit.Rank || (Units[i].Rank == unit.Rank && Units[i].GetRating() > unit.GetRating())))
			{
				unit = Units[i];
			}
		}
		return unit;
	}

	public void CheckUnitStatus()
	{
		int num = 0;
		for (int i = 0; i < Warband.Units.Count; i++)
		{
			UnitActiveStatusId activeStatus = Warband.Units[i].GetActiveStatus();
			if (activeStatus == UnitActiveStatusId.TREATMENT_NOT_PAID || activeStatus == UnitActiveStatusId.INJURED || activeStatus == UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID)
			{
				num++;
			}
		}
		if (num >= 6)
		{
			PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.MULTIPLE_INJURED);
		}
	}
}
