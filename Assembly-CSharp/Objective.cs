using System.Collections.Generic;
using UnityEngine;

public abstract class Objective
{
	public uint guid;

	public string name;

	public string desc;

	public List<string> subDesc;

	public List<bool> dones;

	public Vector2 counter;

	public Vector2 oldCounter;

	public bool done;

	private bool locked;

	private PrimaryObjectiveData data;

	private PrimaryObjectiveTypeData typeData;

	protected List<SearchPoint> searchToCheck;

	protected List<UnitController> unitsToCheck;

	protected List<Item> itemsToSteal = new List<Item>();

	public PrimaryObjectiveId Id => data.Id;

	public PrimaryObjectiveTypeId TypeId => data.PrimaryObjectiveTypeId;

	public string NameKey => "mission_obj_" + data.Name;

	public string DescKey => "mission_obj_" + data.Name + "_desc";

	public PrimaryObjectiveResultId ResultId
	{
		get
		{
			if (done)
			{
				return PrimaryObjectiveResultId.SUCCESS;
			}
			return (!typeData.Mandatory) ? PrimaryObjectiveResultId.PROGRESS : PrimaryObjectiveResultId.FAILED;
		}
	}

	public bool Locked => locked;

	public List<Objective> RequiredObjectives
	{
		get;
		private set;
	}

	public List<bool> RequiredCompleteds
	{
		get;
		private set;
	}

	public int SortWeight
	{
		get;
		private set;
	}

	public Objective(PrimaryObjectiveId id)
	{
		if (PandoraSingleton<MissionManager>.Exists())
		{
			guid = PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID();
		}
		data = PandoraSingleton<DataFactory>.Instance.InitData<PrimaryObjectiveData>((int)id);
		typeData = PandoraSingleton<DataFactory>.Instance.InitData<PrimaryObjectiveTypeData>((int)data.PrimaryObjectiveTypeId);
		counter = Vector2.zero;
		done = false;
		subDesc = new List<string>();
		dones = new List<bool>();
		itemsToSteal = new List<Item>();
		searchToCheck = new List<SearchPoint>();
		unitsToCheck = new List<UnitController>();
		RequiredObjectives = new List<Objective>();
		RequiredCompleteds = new List<bool>();
		name = PandoraSingleton<LocalizationManager>.Instance.GetStringById(NameKey);
		desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(DescKey);
	}

	public static List<Objective> CreateMissionObjectives(CampaignMissionId missionId, WarbandController warCtrlr)
	{
		List<Objective> list = new List<Objective>();
		DataFactory instance = PandoraSingleton<DataFactory>.Instance;
		int num = (int)missionId;
		List<CampaignMissionObjectiveData> list2 = instance.InitData<CampaignMissionObjectiveData>("fk_campaign_mission_id", num.ToString());
		List<Objective> objectives = new List<Objective>();
		for (int i = 0; i < list2.Count; i++)
		{
			objectives.Clear();
			CreateObjective(ref objectives, list2[i].PrimaryObjectiveId, warCtrlr);
			for (int j = 0; j < objectives.Count; j++)
			{
				objectives[j].SortWeight = list2[i].SortWeight + j;
			}
			list.AddRange(objectives);
		}
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			Objective objective = list[num2];
			List<PrimaryObjectiveRequirementData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<PrimaryObjectiveRequirementData>("fk_primary_objective_id", ((int)objective.Id).ToString());
			objective.SetLocked(list3.Count > 0);
			for (int k = 0; k < list3.Count; k++)
			{
				for (int l = 0; l < list.Count; l++)
				{
					if (list[l].Id == list3[k].PrimaryObjectiveIdRequired)
					{
						objective.RequiredObjectives.Add(list[l]);
						objective.RequiredCompleteds.Add(list3[k].RequiredCompleted);
					}
				}
			}
		}
		list.Sort(new ObjectiveComparer());
		return list;
	}

	private static PrimaryObjectiveId GetRandomObjective(PrimaryObjectiveTypeId typeId, int seed)
	{
		Tyche tyche = new Tyche(seed);
		DataFactory instance = PandoraSingleton<DataFactory>.Instance;
		int num = (int)typeId;
		List<PrimaryObjectiveData> list = instance.InitData<PrimaryObjectiveData>("fk_primary_objective_type_id", num.ToString());
		int index = tyche.Rand(0, list.Count);
		return list[index].Id;
	}

	public static void CreateObjective(ref List<Objective> objectives, PrimaryObjectiveTypeId objectiveTypeId, WarbandController warCtrlr, int objectiveSeed = 0, WarbandId enemyWarbandId = WarbandId.NONE, List<Unit> enemies = null, WarbandController enemyWarCtrlr = null)
	{
		CreateObjective(ref objectives, GetRandomObjective(objectiveTypeId, objectiveSeed), warCtrlr, objectiveSeed, enemyWarbandId, enemies, enemyWarCtrlr);
	}

	public static void CreateObjective(ref List<Objective> objectives, PrimaryObjectiveId id, WarbandController warCtrlr, int objectiveSeed = 0, WarbandId enemyWarbandId = WarbandId.NONE, List<Unit> enemies = null, WarbandController enemyWarCtrlr = null)
	{
		PrimaryObjectiveData primaryObjectiveData = PandoraSingleton<DataFactory>.Instance.InitData<PrimaryObjectiveData>((int)id);
		switch (primaryObjectiveData.PrimaryObjectiveTypeId)
		{
		case PrimaryObjectiveTypeId.BOUNTY:
			objectives.Add(new ObjectiveBounty(id, warCtrlr, objectiveSeed, enemies));
			break;
		case PrimaryObjectiveTypeId.GRAND_THEFT_CART:
			objectives.Add(new ObjectiveGranTheftCart(id, warCtrlr, enemyWarbandId, enemyWarCtrlr));
			objectives.Add(new ObjectiveProtectIdol(PrimaryObjectiveId.PROTECT_IDOL, warCtrlr));
			break;
		case PrimaryObjectiveTypeId.WYRDSTONE_RUSH:
			objectives.Add(new ObjectiveWyrdstoneRush(id, warCtrlr, objectiveSeed));
			break;
		case PrimaryObjectiveTypeId.ACTIVATE:
			objectives.Add(new ObjectiveActivate(id));
			break;
		case PrimaryObjectiveTypeId.CONVERT:
			objectives.Add(new ObjectiveConvert(id));
			break;
		case PrimaryObjectiveTypeId.GATHER_INSTALL:
			objectives.Add(new ObjectiveGatherInstall(id, warCtrlr));
			break;
		case PrimaryObjectiveTypeId.LOCATE:
			objectives.Add(new ObjectiveLocate(id));
			break;
		case PrimaryObjectiveTypeId.WANTED:
			objectives.Add(new ObjectiveWanted(id, warCtrlr));
			break;
		case PrimaryObjectiveTypeId.KEEP_ALIVE:
			objectives.Add(new ObjectiveKeepAlive(id));
			break;
		case PrimaryObjectiveTypeId.PROTECT_IDOL:
			objectives.Add(new ObjectiveProtectIdol(id, warCtrlr));
			break;
		case PrimaryObjectiveTypeId.DESTROY:
			objectives.Add(new ObjectiveDestroy(id));
			break;
		default:
			PandoraDebug.LogWarning("Objective type " + primaryObjectiveData.PrimaryObjectiveTypeId + " not supported");
			break;
		}
	}

	public static void CreateLoadingObjective(ref List<Objective> objectives, PrimaryObjectiveTypeId typeId, MissionWarbandSave enemyWarband, int seed)
	{
		PrimaryObjectiveId randomObjective = GetRandomObjective(typeId, seed);
		switch (typeId)
		{
		case PrimaryObjectiveTypeId.NONE:
			break;
		case PrimaryObjectiveTypeId.BOUNTY:
			objectives.Add(new ObjectiveBounty(randomObjective, enemyWarband.Units, seed));
			break;
		case PrimaryObjectiveTypeId.GRAND_THEFT_CART:
			objectives.Add(new ObjectiveGranTheftCart(randomObjective, enemyWarband.WarbandId));
			objectives.Add(new ObjectiveProtectIdol(PrimaryObjectiveId.PROTECT_IDOL, enemyWarband.WarbandId));
			break;
		case PrimaryObjectiveTypeId.WYRDSTONE_RUSH:
			objectives.Add(new ObjectiveWyrdstoneRush(randomObjective, seed));
			break;
		default:
			PandoraDebug.LogWarning("Objective type " + typeId + " not supported");
			break;
		}
	}

	public bool CheckObjective()
	{
		bool objectivesChanged = false;
		bool flag = false;
		oldCounter = counter;
		Track(ref objectivesChanged);
		counter.x = Mathf.Min(counter.x, counter.y);
		objectivesChanged |= (oldCounter.x != counter.x);
		objectivesChanged |= (done != flag);
		flag = (done = (counter.x >= counter.y));
		return objectivesChanged;
	}

	protected abstract void Track(ref bool objectivesChanged);

	public abstract void Reload(uint trackedUid);

	public virtual void SetLocked(bool loc)
	{
		locked = loc;
	}

	protected void CheckItemsToSteal(ref bool objectivesChanged)
	{
		counter.x = 0f;
		for (int i = 0; i < itemsToSteal.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < searchToCheck.Count; j++)
			{
				if (flag)
				{
					break;
				}
				flag = searchToCheck[j].Contains(itemsToSteal[i]);
			}
			if (!flag && unitsToCheck != null)
			{
				for (int k = 0; k < unitsToCheck.Count; k++)
				{
					if (flag)
					{
						break;
					}
					flag = (unitsToCheck[k].unit.Status != UnitStateId.OUT_OF_ACTION && unitsToCheck[k].unit.Items.Contains(itemsToSteal[i]));
				}
			}
			if (dones.Count != 0)
			{
				if (dones[i] != flag)
				{
					objectivesChanged = true;
				}
				dones[i] = flag;
			}
			if (flag)
			{
				counter.x += 1f;
			}
		}
	}
}
