using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MissionEndDataSave : IThoth
{
	public int lastVersion;

	public ProcMissionRatingId ratingId;

	public bool won;

	public bool primaryObjectiveCompleted;

	public bool crushed;

	public bool isCampaign;

	public bool isSkirmish;

	public bool isVsAI;

	public List<MissionEndUnitSave> units;

	public Chest wagonItems = new Chest();

	public int playerMVUIdx;

	public int enemyMVUIdx = -1;

	public MissionSave missionSave = new MissionSave(Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE));

	public bool routable;

	public int seed;

	public List<MissionWarbandSave> missionWarbands = new List<MissionWarbandSave>();

	public List<Tuple<int, int, bool>> warbandMorals = new List<Tuple<int, int, bool>>();

	public List<KeyValuePair<uint, SearchSave>> searches = new List<KeyValuePair<uint, SearchSave>>();

	public List<uint> destroyedTraps = new List<uint>();

	public List<EndZoneAoe> aoeZones = new List<EndZoneAoe>();

	public List<uint> myrtilusLadder = new List<uint>();

	public int currentLadderIdx;

	public int currentTurn;

	public bool missionFinished;

	public List<KeyValuePair<int, int>> reinforcements = new List<KeyValuePair<int, int>>();

	public List<KeyValuePair<uint, uint>> objectives = new List<KeyValuePair<uint, uint>>();

	public List<KeyValuePair<uint, int>> converters = new List<KeyValuePair<uint, int>>();

	public List<KeyValuePair<uint, bool>> activaters = new List<KeyValuePair<uint, bool>>();

	public List<EndDynamicTrap> dynamicTraps = new List<EndDynamicTrap>();

	public List<EndDestructible> destructibles = new List<EndDestructible>();

	public VictoryTypeId VictoryType
	{
		get
		{
			VictoryTypeId result = VictoryTypeId.LOSS;
			if (isCampaign)
			{
				if (primaryObjectiveCompleted)
				{
					result = VictoryTypeId.CAMPAIGN;
				}
			}
			else if (won && !primaryObjectiveCompleted)
			{
				result = VictoryTypeId.BATTLEGROUND;
			}
			else if (!won && primaryObjectiveCompleted)
			{
				result = VictoryTypeId.OBJECTIVE;
			}
			else if (won && primaryObjectiveCompleted)
			{
				result = VictoryTypeId.DECISIVE;
			}
			return result;
		}
	}

	void IThoth.Read(BinaryReader reader)
	{
		int i = 0;
		Thoth.Read(reader, out int i2);
		lastVersion = i2;
		Thoth.Read(reader, out i);
		if (i2 < 2)
		{
			Thoth.Read(reader, out int _);
		}
		int i4 = 0;
		Thoth.Read(reader, out i4);
		ratingId = (ProcMissionRatingId)i4;
		Thoth.Read(reader, out won);
		Thoth.Read(reader, out primaryObjectiveCompleted);
		Thoth.Read(reader, out crushed);
		Thoth.Read(reader, out isCampaign);
		Thoth.Read(reader, out isSkirmish);
		Thoth.Read(reader, out isVsAI);
		int i5 = 0;
		Thoth.Read(reader, out i5);
		units = new List<MissionEndUnitSave>(i5);
		for (int j = 0; j < i5; j++)
		{
			MissionEndUnitSave missionEndUnitSave = new MissionEndUnitSave();
			((IThoth)missionEndUnitSave).Read(reader);
			units.Add(missionEndUnitSave);
		}
		Thoth.Read(reader, out i5);
		for (int k = 0; k < i5; k++)
		{
			ItemSave itemSave = new ItemSave(ItemId.NONE);
			((IThoth)itemSave).Read(reader);
			wagonItems.AddItem(itemSave);
		}
		Thoth.Read(reader, out playerMVUIdx);
		Thoth.Read(reader, out enemyMVUIdx);
		if (i2 == 0)
		{
			Thoth.Read(reader, out int _);
		}
		else
		{
			Thoth.Read(reader, out routable);
		}
		((IThoth)missionSave).Read(reader);
		if (i2 <= 2)
		{
			return;
		}
		Thoth.Read(reader, out seed);
		i5 = 0;
		Thoth.Read(reader, out i5);
		for (int l = 0; l < i5; l++)
		{
			MissionWarbandSave missionWarbandSave = new MissionWarbandSave();
			((IThoth)missionWarbandSave).Read(reader);
			missionWarbands.Add(missionWarbandSave);
		}
		i5 = 0;
		Thoth.Read(reader, out i5);
		for (int m = 0; m < i5; m++)
		{
			uint i7 = 0u;
			Thoth.Read(reader, out i7);
			destroyedTraps.Add(i7);
		}
		i5 = 0;
		Thoth.Read(reader, out i5);
		for (int n = 0; n < i5; n++)
		{
			uint i8 = 0u;
			Thoth.Read(reader, out i8);
			uint i9 = 0u;
			Vector3 zero = Vector3.zero;
			if (i2 > 3)
			{
				Thoth.Read(reader, out i9);
				Thoth.Read(reader, out zero.x);
				Thoth.Read(reader, out zero.y);
				Thoth.Read(reader, out zero.z);
			}
			int i10 = 0;
			Thoth.Read(reader, out i10);
			List<ItemSave> list = new List<ItemSave>(i10);
			for (int num = 0; num < i10; num++)
			{
				ItemSave itemSave2 = new ItemSave(ItemId.NONE);
				((IThoth)itemSave2).Read(reader);
				list.Add(itemSave2);
			}
			bool b = false;
			if (lastVersion > 11)
			{
				Thoth.Read(reader, out b);
			}
			SearchSave searchSave = new SearchSave();
			searchSave.unitCtrlrUid = i9;
			searchSave.pos = zero;
			searchSave.items = ((list.Count <= 0) ? null : list);
			searchSave.wasSearched = b;
			searches.Add(new KeyValuePair<uint, SearchSave>(i8, searchSave));
		}
		i5 = 0;
		Thoth.Read(reader, out i5);
		for (int num2 = 0; num2 < i5; num2++)
		{
			EndZoneAoe item = default(EndZoneAoe);
			Thoth.Read(reader, out item.guid);
			Thoth.Read(reader, out item.myrtilusId);
			int i11 = 0;
			Thoth.Read(reader, out i11);
			item.aoeId = (ZoneAoeId)i11;
			Thoth.Read(reader, out item.radius);
			Thoth.Read(reader, out item.durationLeft);
			Thoth.Read(reader, out item.position.x);
			Thoth.Read(reader, out item.position.y);
			Thoth.Read(reader, out item.position.z);
			aoeZones.Add(item);
		}
		i5 = 0;
		Thoth.Read(reader, out i5);
		myrtilusLadder = new List<uint>();
		for (int num3 = 0; num3 < i5; num3++)
		{
			Thoth.Read(reader, out uint i12);
			myrtilusLadder.Add(i12);
		}
		Thoth.Read(reader, out currentLadderIdx);
		Thoth.Read(reader, out currentTurn);
		i5 = 0;
		Thoth.Read(reader, out i5);
		warbandMorals = new List<Tuple<int, int, bool>>();
		for (int num4 = 0; num4 < i5; num4++)
		{
			int i13 = 0;
			int i14 = 0;
			bool b2 = false;
			Thoth.Read(reader, out i13);
			Thoth.Read(reader, out i14);
			Thoth.Read(reader, out b2);
			Tuple<int, int, bool> item2 = new Tuple<int, int, bool>(i13, i14, b2);
			warbandMorals.Add(item2);
		}
		Thoth.Read(reader, out missionFinished);
		Thoth.Read(reader, out i5);
		for (int num5 = 0; num5 < i5; num5++)
		{
			int i15 = 0;
			Thoth.Read(reader, out i15);
			int i16 = 0;
			Thoth.Read(reader, out i16);
			KeyValuePair<int, int> item3 = new KeyValuePair<int, int>(i15, i16);
			reinforcements.Add(item3);
		}
		Thoth.Read(reader, out i5);
		for (int num6 = 0; num6 < i5; num6++)
		{
			Thoth.Read(reader, out uint i17);
			Thoth.Read(reader, out uint i18);
			objectives.Add(new KeyValuePair<uint, uint>(i17, i18));
		}
		Thoth.Read(reader, out i5);
		for (int num7 = 0; num7 < i5; num7++)
		{
			Thoth.Read(reader, out uint i19);
			Thoth.Read(reader, out int i20);
			converters.Add(new KeyValuePair<uint, int>(i19, i20));
		}
		Thoth.Read(reader, out i5);
		for (int num8 = 0; num8 < i5; num8++)
		{
			Thoth.Read(reader, out uint i21);
			Thoth.Read(reader, out bool b3);
			activaters.Add(new KeyValuePair<uint, bool>(i21, b3));
		}
		if (i2 > 6)
		{
			Thoth.Read(reader, out i5);
			for (int num9 = 0; num9 < i5; num9++)
			{
				EndDynamicTrap item4 = default(EndDynamicTrap);
				Thoth.Read(reader, out item4.guid);
				Thoth.Read(reader, out item4.trapTypeId);
				Thoth.Read(reader, out item4.teamIdx);
				Thoth.Read(reader, out item4.pos.x);
				Thoth.Read(reader, out item4.pos.y);
				Thoth.Read(reader, out item4.pos.z);
				Thoth.Read(reader, out item4.rot.x);
				Thoth.Read(reader, out item4.rot.y);
				Thoth.Read(reader, out item4.rot.z);
				Thoth.Read(reader, out item4.rot.w);
				dynamicTraps.Add(item4);
			}
		}
		if (i2 > 7)
		{
			Thoth.Read(reader, out i5);
			for (int num10 = 0; num10 < i5; num10++)
			{
				EndDestructible item5 = default(EndDestructible);
				Thoth.Read(reader, out item5.guid);
				int i22 = 0;
				Thoth.Read(reader, out i22);
				item5.destructibleId = (DestructibleId)i22;
				Thoth.Read(reader, out item5.onwerGuid);
				Thoth.Read(reader, out item5.wounds);
				Thoth.Read(reader, out item5.position.x);
				Thoth.Read(reader, out item5.position.y);
				Thoth.Read(reader, out item5.position.z);
				Thoth.Read(reader, out item5.rot.x);
				Thoth.Read(reader, out item5.rot.y);
				Thoth.Read(reader, out item5.rot.z);
				Thoth.Read(reader, out item5.rot.w);
				destructibles.Add(item5);
			}
		}
	}

	public int GetVersion()
	{
		return 12;
	}

	public int GetCRC(bool read)
	{
		return CalculateCRC(read);
	}

	public void AddUnits(int count)
	{
		units = new List<MissionEndUnitSave>();
	}

	public void UpdateMoral(int idx, int moral, int oldMoral, bool idolMoral)
	{
		Tuple<int, int, bool> value = new Tuple<int, int, bool>(moral, oldMoral, idolMoral);
		warbandMorals[idx] = value;
	}

	public void UpdateUnit(UnitController unit)
	{
		if (units == null || unit == null)
		{
			return;
		}
		for (int i = 0; i < units.Count; i++)
		{
			if (units[i].myrtilusId == unit.uid)
			{
				units[i].UpdateUnit(unit);
				return;
			}
		}
		MissionEndUnitSave missionEndUnitSave = new MissionEndUnitSave();
		missionEndUnitSave.UpdateUnit(unit);
		units.Add(missionEndUnitSave);
		if (unit.unit.UnitSave.isReinforcement)
		{
			WarbandController warbandController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[unit.unit.warbandIdx];
			KeyValuePair<int, int> item = new KeyValuePair<int, int>(units.Count - 1, warbandController.saveIdx);
			reinforcements.Add(item);
		}
	}

	public void UpdateSearches(uint guid, uint unitControllerUid, Vector3 pos, List<ItemSave> items, bool wasSearched)
	{
		SearchSave searchSave = null;
		for (int i = 0; i < searches.Count; i++)
		{
			if (searches[i].Key == guid)
			{
				searchSave = searches[i].Value;
				searchSave.unitCtrlrUid = unitControllerUid;
				searchSave.pos = pos;
				searchSave.items = items;
				searchSave.wasSearched = wasSearched;
				searches[i] = new KeyValuePair<uint, SearchSave>(guid, searchSave);
				return;
			}
		}
		searchSave = new SearchSave();
		searchSave.unitCtrlrUid = unitControllerUid;
		searchSave.pos = pos;
		searchSave.items = items;
		searchSave.wasSearched = wasSearched;
		searches.Add(new KeyValuePair<uint, SearchSave>(guid, searchSave));
	}

	public void UpdateAoe(uint guid, uint myrtilusId, ZoneAoeId aoeId, float radius, int durationLeft, Vector3 position)
	{
		EndZoneAoe endZoneAoe = default(EndZoneAoe);
		endZoneAoe.myrtilusId = myrtilusId;
		endZoneAoe.guid = guid;
		endZoneAoe.aoeId = aoeId;
		endZoneAoe.radius = radius;
		endZoneAoe.durationLeft = durationLeft;
		endZoneAoe.position = position;
		for (int i = 0; i < aoeZones.Count; i++)
		{
			EndZoneAoe endZoneAoe2 = aoeZones[i];
			if (endZoneAoe2.guid == guid)
			{
				aoeZones[i] = endZoneAoe;
				return;
			}
		}
		aoeZones.Add(endZoneAoe);
	}

	public void ClearAoe(uint guid)
	{
		int num = 0;
		while (true)
		{
			if (num < aoeZones.Count)
			{
				EndZoneAoe endZoneAoe = aoeZones[num];
				if (endZoneAoe.guid == guid)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		aoeZones.RemoveAt(num);
	}

	public void UpdateOpenedSearches(int warbandSaveIdx, uint guid)
	{
		if (missionWarbands[warbandSaveIdx].openedSearches.IndexOf(guid) == -1)
		{
			missionWarbands[warbandSaveIdx].openedSearches.Add(guid);
		}
	}

	public void UpdateObjective(uint uidObj, uint trackedUid = 0)
	{
		objectives.Add(new KeyValuePair<uint, uint>(uidObj, trackedUid));
	}

	public void UpdateConverters(uint guid, int capacity)
	{
		KeyValuePair<uint, int> keyValuePair = new KeyValuePair<uint, int>(guid, capacity);
		for (int i = 0; i < converters.Count; i++)
		{
			if (converters[i].Key == guid)
			{
				converters[i] = keyValuePair;
				return;
			}
		}
		converters.Add(keyValuePair);
	}

	public void UpdateActivated(uint guid, bool status)
	{
		KeyValuePair<uint, bool> keyValuePair = new KeyValuePair<uint, bool>(guid, status);
		for (int i = 0; i < activaters.Count; i++)
		{
			if (activaters[i].Key == guid)
			{
				activaters[i] = keyValuePair;
				return;
			}
		}
		activaters.Add(keyValuePair);
	}

	public void AddDynamicTrap(Trap trap)
	{
		EndDynamicTrap item = default(EndDynamicTrap);
		item.guid = trap.guid;
		item.teamIdx = trap.TeamIdx;
		item.trapTypeId = (int)trap.defaultType;
		item.pos = trap.transform.position;
		item.rot = trap.transform.rotation;
		item.consumed = false;
		dynamicTraps.Add(item);
	}

	public void UpdateDynamicTrap(Trap trap)
	{
		for (int i = 0; i < dynamicTraps.Count; i++)
		{
			EndDynamicTrap endDynamicTrap = dynamicTraps[i];
			if (endDynamicTrap.guid == trap.guid)
			{
				EndDynamicTrap value = dynamicTraps[i];
				value.consumed = true;
				dynamicTraps[i] = value;
			}
		}
	}

	public void AddDestructible(Destructible dest)
	{
		EndDestructible item = default(EndDestructible);
		item.guid = dest.guid;
		item.destructibleId = dest.id;
		item.onwerGuid = ((dest.Owner != null) ? dest.Owner.uid : 0u);
		item.wounds = dest.CurrentWounds;
		item.position = dest.transform.position;
		item.rot = dest.transform.rotation;
		destructibles.Add(item);
	}

	public void UpdateDestructible(Destructible dest)
	{
		for (int i = 0; i < destructibles.Count; i++)
		{
			EndDestructible endDestructible = destructibles[i];
			if (endDestructible.guid == dest.guid)
			{
				EndDestructible value = destructibles[i];
				value.wounds = dest.CurrentWounds;
				destructibles[i] = value;
			}
		}
	}

	private int CalculateCRC(bool read)
	{
		int num = (!read) ? ((IThoth)this).GetVersion() : lastVersion;
		int num2 = 0;
		if (num > 1)
		{
			num2 = (int)(num2 + ratingId);
			num2 = (int)(num2 + ratingId);
			num2 += (won ? 1 : 0);
			num2 += (primaryObjectiveCompleted ? 1 : 0);
			num2 += (crushed ? 1 : 0);
			num2 += (isCampaign ? 1 : 0);
			num2 += (isSkirmish ? 1 : 0);
			num2 += (isVsAI ? 1 : 0);
			List<ItemSave> items = wagonItems.GetItems();
			for (int i = 0; i < items.Count; i++)
			{
				num2 += items[i].GetCRC(read);
			}
			num2 += playerMVUIdx;
			num2 += playerMVUIdx;
			num2 += enemyMVUIdx;
			num2 += missionSave.GetCRC(read);
		}
		if (num > 0)
		{
			num2 += (routable ? 1 : 0);
		}
		if (num > 2)
		{
			num2 += seed;
			for (int j = 0; j < missionWarbands.Count; j++)
			{
				num2 += missionWarbands[j].GetCRC(read);
			}
			for (int k = 0; k < destroyedTraps.Count; k++)
			{
				num2 += (int)destroyedTraps[k];
			}
			for (int l = 0; l < searches.Count; l++)
			{
				num2 += (int)searches[l].Key;
				num2 += (int)searches[l].Value.unitCtrlrUid;
				num2 += (int)searches[l].Value.pos.x;
				num2 += (int)searches[l].Value.pos.y;
				num2 += (int)searches[l].Value.pos.z;
				if (searches[l].Value != null)
				{
					num2 += searches[l].Value.items.Count;
				}
			}
			for (int m = 0; m < aoeZones.Count; m++)
			{
				int num3 = num2;
				EndZoneAoe endZoneAoe = aoeZones[m];
				num2 = num3 + (int)endZoneAoe.guid;
				int num4 = num2;
				EndZoneAoe endZoneAoe2 = aoeZones[m];
				num2 = num4 + (int)endZoneAoe2.myrtilusId;
				int num5 = num2;
				EndZoneAoe endZoneAoe3 = aoeZones[m];
				num2 = (int)(num5 + endZoneAoe3.aoeId);
				int num6 = num2;
				EndZoneAoe endZoneAoe4 = aoeZones[m];
				num2 = num6 + (int)endZoneAoe4.radius;
				int num7 = num2;
				EndZoneAoe endZoneAoe5 = aoeZones[m];
				num2 = num7 + endZoneAoe5.durationLeft;
				int num8 = num2;
				EndZoneAoe endZoneAoe6 = aoeZones[m];
				num2 = num8 + (int)endZoneAoe6.position.x;
				int num9 = num2;
				EndZoneAoe endZoneAoe7 = aoeZones[m];
				num2 = num9 + (int)endZoneAoe7.position.y;
				int num10 = num2;
				EndZoneAoe endZoneAoe8 = aoeZones[m];
				num2 = num10 + (int)endZoneAoe8.position.z;
			}
			for (int n = 0; n < myrtilusLadder.Count; n++)
			{
				num2 += (int)myrtilusLadder[n];
			}
			num2 += currentLadderIdx;
			num2 += currentTurn;
			for (int num11 = 0; num11 < warbandMorals.Count; num11++)
			{
				num2 += warbandMorals[num11].Item1;
				num2 += warbandMorals[num11].Item2;
				num2 += (warbandMorals[num11].Item3 ? 1 : 0);
			}
			num2 += (missionFinished ? 1 : 0);
			for (int num12 = 0; num12 < reinforcements.Count; num12++)
			{
				num2 += reinforcements[num12].Key;
				num2 += reinforcements[num12].Value;
			}
			for (int num13 = 0; num13 < objectives.Count; num13++)
			{
				num2 += (int)objectives[num13].Key;
				num2 += (int)objectives[num13].Value;
			}
			for (int num14 = 0; num14 < converters.Count; num14++)
			{
				num2 += (int)converters[num14].Key;
				num2 += converters[num14].Value;
			}
			for (int num15 = 0; num15 < activaters.Count; num15++)
			{
				num2 += (int)activaters[num15].Key;
				num2 += (activaters[num15].Value ? 1 : 0);
			}
			if (num > 6)
			{
				for (int num16 = 0; num16 < dynamicTraps.Count; num16++)
				{
					int num17 = num2;
					EndDynamicTrap endDynamicTrap = dynamicTraps[num16];
					num2 = num17 + endDynamicTrap.trapTypeId;
					int num18 = num2;
					EndDynamicTrap endDynamicTrap2 = dynamicTraps[num16];
					num2 = num18 + endDynamicTrap2.teamIdx;
					int num19 = num2;
					EndDynamicTrap endDynamicTrap3 = dynamicTraps[num16];
					num2 = num19 + (int)endDynamicTrap3.pos.x;
					int num20 = num2;
					EndDynamicTrap endDynamicTrap4 = dynamicTraps[num16];
					num2 = num20 + (int)endDynamicTrap4.pos.y;
					int num21 = num2;
					EndDynamicTrap endDynamicTrap5 = dynamicTraps[num16];
					num2 = num21 + (int)endDynamicTrap5.pos.z;
					int num22 = num2;
					EndDynamicTrap endDynamicTrap6 = dynamicTraps[num16];
					num2 = num22 + (int)endDynamicTrap6.rot.x;
					int num23 = num2;
					EndDynamicTrap endDynamicTrap7 = dynamicTraps[num16];
					num2 = num23 + (int)endDynamicTrap7.rot.y;
					int num24 = num2;
					EndDynamicTrap endDynamicTrap8 = dynamicTraps[num16];
					num2 = num24 + (int)endDynamicTrap8.rot.z;
					int num25 = num2;
					EndDynamicTrap endDynamicTrap9 = dynamicTraps[num16];
					num2 = num25 + (int)endDynamicTrap9.rot.w;
				}
			}
			if (num > 7)
			{
				for (int num26 = 0; num26 < destructibles.Count; num26++)
				{
					int num27 = num2;
					EndDestructible endDestructible = destructibles[num26];
					num2 = (int)(num27 + endDestructible.destructibleId);
					int num28 = num2;
					EndDestructible endDestructible2 = destructibles[num26];
					num2 = num28 + endDestructible2.wounds;
					int num29 = num2;
					EndDestructible endDestructible3 = destructibles[num26];
					num2 = num29 + (int)endDestructible3.onwerGuid;
					int num30 = num2;
					EndDestructible endDestructible4 = destructibles[num26];
					num2 = num30 + (int)endDestructible4.position.x;
					int num31 = num2;
					EndDestructible endDestructible5 = destructibles[num26];
					num2 = num31 + (int)endDestructible5.position.y;
					int num32 = num2;
					EndDestructible endDestructible6 = destructibles[num26];
					num2 = num32 + (int)endDestructible6.position.z;
					int num33 = num2;
					EndDestructible endDestructible7 = destructibles[num26];
					num2 = num33 + (int)endDestructible7.rot.x;
					int num34 = num2;
					EndDestructible endDestructible8 = destructibles[num26];
					num2 = num34 + (int)endDestructible8.rot.y;
					int num35 = num2;
					EndDestructible endDestructible9 = destructibles[num26];
					num2 = num35 + (int)endDestructible9.rot.z;
					int num36 = num2;
					EndDestructible endDestructible10 = destructibles[num26];
					num2 = num36 + (int)endDestructible10.rot.w;
				}
			}
		}
		return num2;
	}

	public void Write(BinaryWriter writer)
	{
		int version = ((IThoth)this).GetVersion();
		Thoth.Write(writer, version);
		int cRC = GetCRC(read: false);
		Thoth.Write(writer, cRC);
		Thoth.Write(writer, (int)ratingId);
		Thoth.Write(writer, won);
		Thoth.Write(writer, primaryObjectiveCompleted);
		Thoth.Write(writer, crushed);
		Thoth.Write(writer, isCampaign);
		Thoth.Write(writer, isSkirmish);
		Thoth.Write(writer, isVsAI);
		Thoth.Write(writer, units.Count);
		for (int i = 0; i < units.Count; i++)
		{
			units[i].Write(writer);
		}
		Thoth.Write(writer, wagonItems.GetItems().Count);
		for (int j = 0; j < wagonItems.GetItems().Count; j++)
		{
			((IThoth)wagonItems.GetItems()[j]).Write(writer);
		}
		Thoth.Write(writer, playerMVUIdx);
		Thoth.Write(writer, enemyMVUIdx);
		Thoth.Write(writer, routable);
		((IThoth)missionSave).Write(writer);
		Thoth.Write(writer, seed);
		Thoth.Write(writer, missionWarbands.Count);
		for (int k = 0; k < missionWarbands.Count; k++)
		{
			missionWarbands[k].Write(writer);
		}
		Thoth.Write(writer, destroyedTraps.Count);
		for (int l = 0; l < destroyedTraps.Count; l++)
		{
			Thoth.Write(writer, destroyedTraps[l]);
		}
		Thoth.Write(writer, searches.Count);
		for (int m = 0; m < searches.Count; m++)
		{
			Thoth.Write(writer, searches[m].Key);
			if (searches[m].Value != null)
			{
				Thoth.Write(writer, searches[m].Value.unitCtrlrUid);
				Thoth.Write(writer, searches[m].Value.pos.x);
				Thoth.Write(writer, searches[m].Value.pos.y);
				Thoth.Write(writer, searches[m].Value.pos.z);
				Thoth.Write(writer, searches[m].Value.items.Count);
				for (int n = 0; n < searches[m].Value.items.Count; n++)
				{
					((IThoth)searches[m].Value.items[n]).Write(writer);
				}
				Thoth.Write(writer, searches[m].Value.wasSearched);
			}
			else
			{
				Thoth.Write(writer, 0);
			}
		}
		Thoth.Write(writer, aoeZones.Count);
		for (int num = 0; num < aoeZones.Count; num++)
		{
			EndZoneAoe endZoneAoe = aoeZones[num];
			Thoth.Write(writer, endZoneAoe.guid);
			EndZoneAoe endZoneAoe2 = aoeZones[num];
			Thoth.Write(writer, endZoneAoe2.myrtilusId);
			EndZoneAoe endZoneAoe3 = aoeZones[num];
			Thoth.Write(writer, (int)endZoneAoe3.aoeId);
			EndZoneAoe endZoneAoe4 = aoeZones[num];
			Thoth.Write(writer, endZoneAoe4.radius);
			EndZoneAoe endZoneAoe5 = aoeZones[num];
			Thoth.Write(writer, endZoneAoe5.durationLeft);
			EndZoneAoe endZoneAoe6 = aoeZones[num];
			Thoth.Write(writer, endZoneAoe6.position.x);
			EndZoneAoe endZoneAoe7 = aoeZones[num];
			Thoth.Write(writer, endZoneAoe7.position.y);
			EndZoneAoe endZoneAoe8 = aoeZones[num];
			Thoth.Write(writer, endZoneAoe8.position.z);
		}
		Thoth.Write(writer, myrtilusLadder.Count);
		for (int num2 = 0; num2 < myrtilusLadder.Count; num2++)
		{
			Thoth.Write(writer, myrtilusLadder[num2]);
		}
		Thoth.Write(writer, currentLadderIdx);
		Thoth.Write(writer, currentTurn);
		Thoth.Write(writer, warbandMorals.Count);
		for (int num3 = 0; num3 < warbandMorals.Count; num3++)
		{
			Thoth.Write(writer, warbandMorals[num3].Item1);
			Thoth.Write(writer, warbandMorals[num3].Item2);
			Thoth.Write(writer, warbandMorals[num3].Item3);
		}
		Thoth.Write(writer, missionFinished);
		Thoth.Write(writer, reinforcements.Count);
		for (int num4 = 0; num4 < reinforcements.Count; num4++)
		{
			Thoth.Write(writer, reinforcements[num4].Key);
			Thoth.Write(writer, reinforcements[num4].Value);
		}
		Thoth.Write(writer, objectives.Count);
		for (int num5 = 0; num5 < objectives.Count; num5++)
		{
			Thoth.Write(writer, objectives[num5].Key);
			Thoth.Write(writer, objectives[num5].Value);
		}
		Thoth.Write(writer, converters.Count);
		for (int num6 = 0; num6 < converters.Count; num6++)
		{
			Thoth.Write(writer, converters[num6].Key);
			Thoth.Write(writer, converters[num6].Value);
		}
		Thoth.Write(writer, activaters.Count);
		for (int num7 = 0; num7 < activaters.Count; num7++)
		{
			Thoth.Write(writer, activaters[num7].Key);
			Thoth.Write(writer, activaters[num7].Value);
		}
		Thoth.Write(writer, dynamicTraps.Count);
		for (int num8 = 0; num8 < dynamicTraps.Count; num8++)
		{
			EndDynamicTrap endDynamicTrap = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap.guid);
			EndDynamicTrap endDynamicTrap2 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap2.trapTypeId);
			EndDynamicTrap endDynamicTrap3 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap3.teamIdx);
			EndDynamicTrap endDynamicTrap4 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap4.pos.x);
			EndDynamicTrap endDynamicTrap5 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap5.pos.y);
			EndDynamicTrap endDynamicTrap6 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap6.pos.z);
			EndDynamicTrap endDynamicTrap7 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap7.rot.x);
			EndDynamicTrap endDynamicTrap8 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap8.rot.y);
			EndDynamicTrap endDynamicTrap9 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap9.rot.z);
			EndDynamicTrap endDynamicTrap10 = dynamicTraps[num8];
			Thoth.Write(writer, endDynamicTrap10.rot.w);
		}
		Thoth.Write(writer, destructibles.Count);
		for (int num9 = 0; num9 < destructibles.Count; num9++)
		{
			EndDestructible endDestructible = destructibles[num9];
			Thoth.Write(writer, endDestructible.guid);
			EndDestructible endDestructible2 = destructibles[num9];
			Thoth.Write(writer, (int)endDestructible2.destructibleId);
			EndDestructible endDestructible3 = destructibles[num9];
			Thoth.Write(writer, endDestructible3.onwerGuid);
			EndDestructible endDestructible4 = destructibles[num9];
			Thoth.Write(writer, endDestructible4.wounds);
			EndDestructible endDestructible5 = destructibles[num9];
			Thoth.Write(writer, endDestructible5.position.x);
			EndDestructible endDestructible6 = destructibles[num9];
			Thoth.Write(writer, endDestructible6.position.y);
			EndDestructible endDestructible7 = destructibles[num9];
			Thoth.Write(writer, endDestructible7.position.z);
			EndDestructible endDestructible8 = destructibles[num9];
			Thoth.Write(writer, endDestructible8.rot.x);
			EndDestructible endDestructible9 = destructibles[num9];
			Thoth.Write(writer, endDestructible9.rot.y);
			EndDestructible endDestructible10 = destructibles[num9];
			Thoth.Write(writer, endDestructible10.rot.z);
			EndDestructible endDestructible11 = destructibles[num9];
			Thoth.Write(writer, endDestructible11.rot.w);
		}
	}
}
