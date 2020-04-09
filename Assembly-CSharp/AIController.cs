using Pathfinding;
using RAIN.BehaviorTrees;
using RAIN.Core;
using RAIN.Minds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIController
{
	private enum BestPathType
	{
		CLOSEST,
		FAREST
	}

	public const int ACTION_MAX_TARGET = 2;

	public const int MIN_ROLL = 50;

	public static List<UnitActionId> attackActions;

	public static List<UnitActionId> spellActions;

	public static List<UnitActionId> consSkillActions;

	public static List<UnitActionId> consSkillSpellActions;

	public static List<UnitActionId> chargeActions;

	public static List<UnitActionId> shootActions;

	public static List<UnitActionId> stanceActions;

	private UnitController unitCtrlr;

	public AiProfileId aiProfileId;

	private AiUnitId previousAIId;

	public Path currentPath;

	public bool movingToSearchPoint;

	public SearchPoint targetSearchPoint;

	public UnitController targetEnemy;

	public DecisionPoint targetDecisionPoint;

	public Destructible targetDestructible;

	public int failedMove;

	public int switchCount;

	public int disengageCount;

	public bool atDestination;

	public List<SearchPoint> lootedSearchPoints = new List<SearchPoint>();

	public List<UnitController> reachableUnits = new List<UnitController>();

	public Dictionary<UnitController, Path> reachableUnitsPaths = new Dictionary<UnitController, Path>();

	private int pathIdx;

	private float currentPathLg;

	private Path calculatedPath;

	private BestPathType bestPathType;

	private bool checkDecisionOnCannotReach;

	private bool forceDecisionCheck;

	private bool keepOnlyReachable;

	private bool fallBackToOldPath;

	private List<Transform> targets = new List<Transform>();

	private Transform bestTarget;

	private UnityAction PreCheck;

	private UnityAction<bool> PostCheck;

	private UnityAction PathFound;

	private UnityAction CannotReach;

	private UnityAction<bool> AllChecked;

	private List<UnitController> units;

	private List<UnitController> excludedUnits = new List<UnitController>();

	private List<SearchPoint> searchPoints;

	private List<DecisionPoint> decisionPoints = new List<DecisionPoint>();

	private List<Destructible> destructibles = new List<Destructible>();

	private Path oldPath;

	private float maxDist;

	private List<int> hitRolls = new List<int>();

	private Dictionary<SkillId, List<UnitController>> skillTargets = new Dictionary<SkillId, List<UnitController>>();

	private List<SkillId> usedSkillTurn = new List<SkillId>();

	public bool hasCastSkill;

	public bool preFight;

	public bool hasSeenEnemy;

	private List<ActionStatus> allActions = new List<ActionStatus>();

	private List<UnitController> allTargets = new List<UnitController>();

	private List<ActionStatus> validActions = new List<ActionStatus>();

	private List<UnitController> validTargets = new List<UnitController>();

	private List<UnitController> tmpTargets = new List<UnitController>();

	private List<UnitController> defendersCopy = new List<UnitController>();

	private List<SkillAiFilterData> excludeFilters = new List<SkillAiFilterData>();

	private List<SkillAiFilterData> validFilters = new List<SkillAiFilterData>();

	public AI Brain
	{
		get;
		private set;
	}

	public AiProfileData profileData
	{
		get;
		private set;
	}

	public AiUnitData aiUnitData
	{
		get;
		private set;
	}

	public BTAsset bt
	{
		get;
		private set;
	}

	public Squad Squad
	{
		get;
		private set;
	}

	public AIController(UnitController ctrlr)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		unitCtrlr = ctrlr;
		Brain = (AI)(object)new AI();
		Brain.AIInit();
		Brain.set_Body(unitCtrlr.gameObject);
		Brain.set_Mind((RAINMind)(object)new BasicMind());
		((RAINAIElement)Brain.get_Mind()).AIInit(Brain);
		if (ctrlr.unit.CampaignData != null)
		{
			SetAIProfile(ctrlr.unit.CampaignData.AiProfileId);
			return;
		}
		List<UnitRoamingData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitRoamingData>("fk_unit_id", ((int)ctrlr.unit.Id).ToConstantString());
		if (unitCtrlr.unit.Id == UnitId.MANTICORE)
		{
			SetAIProfile(AiProfileId.CAMPAIGN_MANTICORE);
		}
		else if (list != null && list.Count > 0)
		{
			SetAIProfile(list[0].AiProfileId);
		}
		else
		{
			SetAIProfile(AiProfileId.BASE_SKIRMISH);
		}
	}

	static AIController()
	{
		attackActions = new List<UnitActionId>
		{
			UnitActionId.MELEE_ATTACK
		};
		spellActions = new List<UnitActionId>
		{
			UnitActionId.SPELL
		};
		consSkillActions = new List<UnitActionId>
		{
			UnitActionId.CONSUMABLE,
			UnitActionId.SKILL
		};
		consSkillSpellActions = new List<UnitActionId>
		{
			UnitActionId.CONSUMABLE,
			UnitActionId.SPELL,
			UnitActionId.SKILL
		};
		chargeActions = new List<UnitActionId>
		{
			UnitActionId.CHARGE
		};
		shootActions = new List<UnitActionId>
		{
			UnitActionId.SHOOT
		};
		stanceActions = new List<UnitActionId>
		{
			UnitActionId.STANCE
		};
	}

	public void SetAIProfile(AiProfileId profileId)
	{
		aiProfileId = profileId;
		profileData = PandoraSingleton<DataFactory>.Instance.InitData<AiProfileData>((int)aiProfileId);
		SetBT(profileData.AiUnitIdBase);
	}

	public void TurnStartCleanUp()
	{
		switchCount = 0;
		disengageCount = 0;
		atDestination = false;
		failedMove = 0;
		switchCount = 0;
		disengageCount = 0;
		targetEnemy = null;
		targetDecisionPoint = null;
		targetDestructible = null;
		excludedUnits.Clear();
		skillTargets.Clear();
		lootedSearchPoints.Clear();
		usedSkillTurn.Clear();
		hasCastSkill = false;
	}

	public void TurnEndCleanUp()
	{
		hasSeenEnemy = false;
	}

	public void AddEngagedToExcluded()
	{
		if (unitCtrlr.HasClose())
		{
			excludedUnits.AddRange(unitCtrlr.EngagedUnits);
		}
	}

	public void RemoveInactive(List<UnitController> targets)
	{
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			if (!targets[num].isInLadder || targets[num].unit.Status == UnitStateId.OUT_OF_ACTION)
			{
				targets.RemoveAt(num);
			}
		}
	}

	public bool GotoAlternateMode()
	{
		if (profileData.AiUnitIdAlternate != 0 && aiUnitData.Id != profileData.AiUnitIdAlternate)
		{
			SetBT(profileData.AiUnitIdAlternate);
			return true;
		}
		return false;
	}

	public void GotoBaseMode()
	{
		if (aiUnitData.Id != profileData.AiUnitIdBase)
		{
			SetBT(profileData.AiUnitIdBase);
		}
	}

	public void GotoSearchMode()
	{
		if (aiUnitData.Id != profileData.AiUnitIdSearch)
		{
			SetBT(profileData.AiUnitIdSearch);
		}
	}

	public void GotoSkillSpellTargetMode()
	{
		if (aiUnitData.Id != profileData.AiUnitIdSearch)
		{
			SetBT(profileData.AiUnitIdSkillSpellTarget);
		}
	}

	public void GotoPreviousMode()
	{
		if (aiUnitData.Id != previousAIId)
		{
			SetBT(previousAIId);
		}
	}

	private void SetBT(AiUnitId aiId)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		previousAIId = ((aiUnitData != null) ? aiUnitData.Id : AiUnitId.NONE);
		aiUnitData = PandoraSingleton<DataFactory>.Instance.InitData<AiUnitData>((int)aiId);
		bt = (BTAsset)(object)(BTAsset)Object.Instantiate(Resources.Load("ai/" + aiUnitData.Name));
		(Brain.get_Mind() as BasicMind).SetBehavior(bt, PandoraSingleton<TreeBinder>.Instance.BtBindings);
	}

	public void RestartBT()
	{
		(Brain.get_Mind() as BasicMind).ResetBehavior();
	}

	public void FixedUpdate()
	{
		Brain.UpdateTime();
		Brain.Think();
	}

	public bool AlreadyLootSearchPoint(SearchPoint search)
	{
		return lootedSearchPoints.IndexOf(search) != -1;
	}

	public void UpdateVisibility()
	{
		List<UnitController> aliveEnemies = PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(unitCtrlr.GetWarband().idx);
		float @float = Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC);
		unitCtrlr.UpdateTargetsData();
		for (int i = 0; i < aliveEnemies.Count; i++)
		{
			if (unitCtrlr.IsInRange(aliveEnemies[i], 0f, unitCtrlr.unit.ViewDistance, @float, unitBlocking: false, checkAllBones: false, BoneId.NONE))
			{
				unitCtrlr.GetWarband().SquadManager.UnitSpotted(aliveEnemies[i]);
			}
		}
	}

	public void SetSquad(Squad squad, int idx)
	{
		Squad = squad;
	}

	public void FindPath(List<UnitController> unitsToCheck, UnityAction<bool> allChecked, bool onlyReachable)
	{
		if (atDestination)
		{
			allChecked(arg0: false);
			return;
		}
		units = unitsToCheck;
		if (!onlyReachable && !PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
		{
			Squad.RefineTargetsList(units);
		}
		for (int num = units.Count - 1; num >= 0; num--)
		{
			if (excludedUnits.IndexOf(units[num]) != -1)
			{
				units.RemoveAt(num);
			}
		}
		reachableUnits.Clear();
		reachableUnitsPaths.Clear();
		AllChecked = allChecked;
		if (targetEnemy != null && targetEnemy.unit.Status != UnitStateId.OUT_OF_ACTION)
		{
			PandoraDebug.LogInfo("Clear targets and set previous target instead(" + targetEnemy.name + ")", "PATHFINDING");
			units.Clear();
			units.Add(targetEnemy);
		}
		StartPathsCheck(units, BestPathType.CLOSEST, !onlyReachable, unitCtrlr.HasRange(), onlyReachable, fallbackToOld: false, PreCheckUnit, PostCheckUnit, PathFoundUnit, null);
	}

	private void PreCheckUnit()
	{
		UnitController unitController = units[pathIdx];
		unitController.SetGraphWalkability(walkable: true);
		PandoraSingleton<MissionManager>.Instance.pathRayModifier.traversableColliders.Clear();
		PandoraSingleton<MissionManager>.Instance.pathRayModifier.traversableColliders.Add(unitController.GetComponent<Collider>());
		PandoraSingleton<MissionManager>.Instance.pathRayModifier.traversableColliders.Add(unitController.combatCircle.Collider);
		maxDist = (float)(unitCtrlr.unit.CurrentStrategyPoints * unitCtrlr.unit.Movement) + Constant.GetFloat((unitController.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? ConstantId.MELEE_RANGE_NORMAL : ConstantId.MELEE_RANGE_LARGE);
	}

	private void PostCheckUnit(bool reachable)
	{
		if (reachable)
		{
			reachableUnits.Add(units[pathIdx]);
			reachableUnitsPaths[units[pathIdx]] = calculatedPath;
		}
		units[pathIdx].SetGraphWalkability(walkable: false);
	}

	private void PathFoundUnit()
	{
		targetEnemy = units[pathIdx];
	}

	public void FindPath(List<SearchPoint> searchPointsToCheck, UnityAction<bool> allChecked)
	{
		if (atDestination)
		{
			allChecked(arg0: false);
			return;
		}
		searchPoints = searchPointsToCheck;
		AllChecked = allChecked;
		if (targetSearchPoint != null)
		{
			searchPoints.Clear();
			searchPoints.Add(targetSearchPoint);
		}
		maxDist = unitCtrlr.unit.CurrentStrategyPoints * unitCtrlr.unit.Movement;
		StartPathsCheck(searchPoints, BestPathType.CLOSEST, checkDecisionAfter: true, forceDecisionAfter: false, keepReachable: false, fallbackToOld: false, PreCheckSearchPoint, null, PathFoundSearch, CannotReachSearch);
	}

	private void PreCheckSearchPoint()
	{
		int index = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, searchPoints[pathIdx].triggers.Count);
		targets[pathIdx] = searchPoints[pathIdx].triggers[index].transform;
	}

	private void PathFoundSearch()
	{
		movingToSearchPoint = true;
		targetSearchPoint = searchPoints[pathIdx];
	}

	private void CannotReachSearch()
	{
		movingToSearchPoint = false;
		targetSearchPoint = null;
	}

	public void FindPath(DecisionPointId decisionPointId, float dist, UnityAction<bool> allChecked)
	{
		if (atDestination)
		{
			allChecked(arg0: false);
			return;
		}
		AllChecked = allChecked;
		decisionPoints = PandoraSingleton<MissionManager>.Instance.GetDecisionPoints(unitCtrlr, decisionPointId, dist);
		maxDist = unitCtrlr.unit.CurrentStrategyPoints * unitCtrlr.unit.Movement;
		StartPathsCheck(decisionPoints, BestPathType.CLOSEST, checkDecisionAfter: false, forceDecisionAfter: false, keepReachable: true, fallbackToOld: false, null, null, PathFoundTactical, null);
	}

	private void FindPathDecision(Vector3 targetPosition, bool fallBackOnOldPath)
	{
		DecisionPointId decisionPointId = unitCtrlr.HasClose() ? DecisionPointId.AMBUSH : DecisionPointId.OVERWATCH;
		bool keepReachable = true;
		BestPathType pathType = BestPathType.FAREST;
		if (targetDecisionPoint != null)
		{
			decisionPoints.Clear();
			decisionPoints.Add(targetDecisionPoint);
		}
		else if (targetEnemy != null && decisionPointId == DecisionPointId.OVERWATCH)
		{
			keepReachable = false;
			pathType = BestPathType.CLOSEST;
			FindBestOverwatch();
			if (decisionPoints.Count == 0)
			{
				keepReachable = true;
				pathType = BestPathType.FAREST;
				FindDecisionClosePath(decisionPointId);
			}
		}
		else
		{
			FindDecisionClosePath(decisionPointId);
		}
		StartPathsCheck(decisionPoints, pathType, checkDecisionAfter: false, forceDecisionAfter: false, keepReachable, fallBackOnOldPath, null, null, PathFoundTactical, null);
	}

	private void FindDecisionClosePath(DecisionPointId decisionPointId)
	{
		int num = unitCtrlr.unit.CurrentStrategyPoints * unitCtrlr.unit.Movement;
		num *= num;
		decisionPoints = PandoraSingleton<MissionManager>.Instance.GetDecisionPoints(unitCtrlr, decisionPointId, num);
		float num2 = unitCtrlr.unit.Movement * 2;
		num2 *= num2;
		for (int num3 = decisionPoints.Count - 1; num3 >= 0; num3--)
		{
			bool flag = false;
			int num4 = 0;
			while (!flag && num4 < currentPath.vectorPath.Count - 1)
			{
				if (PandoraUtils.SqrDistPointLineDist(currentPath.vectorPath[num4], currentPath.vectorPath[num4 + 1], decisionPoints[num3].transform.position, isSegment: true) < num2)
				{
					flag = true;
				}
				num4++;
			}
			if (!flag)
			{
				decisionPoints.RemoveAt(num3);
			}
		}
	}

	private void FindBestOverwatch()
	{
		float minDistance = 0f;
		float num = 0f;
		if (unitCtrlr.HasRange())
		{
			minDistance = unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.RangeMin;
			num = unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.RangeMax;
		}
		else if (unitCtrlr.IsAltRange())
		{
			minDistance = unitCtrlr.Equipments[(int)unitCtrlr.unit.InactiveWeaponSlot].Item.RangeMin;
			num = unitCtrlr.Equipments[(int)unitCtrlr.unit.InactiveWeaponSlot].Item.RangeMax;
		}
		List<DecisionPoint> list = PandoraSingleton<MissionManager>.Instance.GetDecisionPoints(targetEnemy, DecisionPointId.OVERWATCH, num);
		int num2 = 0;
		decisionPoints.Clear();
		hitRolls.Clear();
		unitCtrlr.defenderCtrlr = targetEnemy;
		TargetData targetData = new TargetData(targetEnemy);
		float @float = Constant.GetFloat(ConstantId.RANGE_SHOOT_REQUIRED_PERC);
		for (int i = 0; i < list.Count; i++)
		{
			DecisionPoint decisionPoint = list[i];
			if (!unitCtrlr.CanTargetFromPoint(targetData, minDistance, num, @float, unitBlocking: true, checkAllBones: true))
			{
				continue;
			}
			int rangeHitRoll = unitCtrlr.GetRangeHitRoll(decisionPoint.transform);
			if (rangeHitRoll > num2)
			{
				num2 = rangeHitRoll;
				for (int num3 = decisionPoints.Count - 1; num3 >= 0; num3--)
				{
					if (hitRolls[num3] < num2 - 10)
					{
						decisionPoints.RemoveAt(num3);
						hitRolls.RemoveAt(num3);
					}
				}
				decisionPoints.Add(decisionPoint);
				hitRolls.Add(rangeHitRoll);
			}
			else if (rangeHitRoll >= num2 - 10)
			{
				decisionPoints.Add(decisionPoint);
				hitRolls.Add(rangeHitRoll);
			}
		}
		unitCtrlr.defenderCtrlr = null;
	}

	private void PathFoundTactical()
	{
		targetDecisionPoint = decisionPoints[pathIdx];
	}

	public void FindPath(List<Destructible> destToCheck, UnityAction<bool> allChecked)
	{
		if (atDestination)
		{
			allChecked(arg0: false);
			return;
		}
		destructibles = destToCheck;
		AllChecked = allChecked;
		if (targetDestructible != null)
		{
			destructibles.Clear();
			destructibles.Add(targetDestructible);
		}
		maxDist = unitCtrlr.unit.CurrentStrategyPoints * unitCtrlr.unit.Movement;
		StartPathsCheck(destructibles, BestPathType.CLOSEST, checkDecisionAfter: true, forceDecisionAfter: false, keepReachable: false, fallbackToOld: false, null, null, PathFoundDestructible, CannotReachDestructible);
	}

	private void PathFoundDestructible()
	{
		targetDestructible = destructibles[pathIdx];
	}

	private void CannotReachDestructible()
	{
		targetDestructible = null;
	}

	private void StartPathsCheck<T>(List<T> behaviours, BestPathType pathType, bool checkDecisionAfter, bool forceDecisionAfter, bool keepReachable, bool fallbackToOld, UnityAction precheck, UnityAction<bool> postCheck, UnityAction pathFound, UnityAction cannotReach) where T : MonoBehaviour
	{
		if (targetDecisionPoint != null && (forceDecisionAfter | checkDecisionAfter))
		{
			FindPathDecision(unitCtrlr.transform.position, fallBackOnOldPath: false);
			return;
		}
		if (behaviours.Count == 0)
		{
			AllChecked(fallbackToOld && currentPath != null);
			return;
		}
		bestPathType = pathType;
		checkDecisionOnCannotReach = checkDecisionAfter;
		forceDecisionCheck = forceDecisionAfter;
		keepOnlyReachable = keepReachable;
		fallBackToOldPath = fallbackToOld;
		PreCheck = precheck;
		PostCheck = postCheck;
		PathFound = pathFound;
		CannotReach = cannotReach;
		targets.Clear();
		for (int i = 0; i < behaviours.Count; i++)
		{
			List<Transform> list = targets;
			T val = behaviours[i];
			list.Add(val.transform);
		}
		pathIdx = 0;
		oldPath = currentPath;
		currentPath = null;
		targetDecisionPoint = null;
		int num = (unitCtrlr.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? 127 : 113;
		if (unitCtrlr.unit.IsUnitActionBlocked(UnitActionId.CLIMB))
		{
			num = (num & -3 & -17);
		}
		if (unitCtrlr.unit.IsUnitActionBlocked(UnitActionId.JUMP))
		{
			num = (num & -5 & -33);
		}
		if (unitCtrlr.unit.IsUnitActionBlocked(UnitActionId.LEAP))
		{
			num = (num & -9 & -65);
		}
		PandoraSingleton<MissionManager>.Instance.PathSeeker.traversableTags = num;
		PandoraSingleton<MissionManager>.Instance.pathRayModifier.SetRadius(unitCtrlr.CapsuleRadius);
		unitCtrlr.StopCoroutine(CheckNext());
		unitCtrlr.ReduceAlliesNavCutterSize(delegate
		{
			unitCtrlr.StartCoroutine(CheckNext());
		});
	}

	private IEnumerator CheckNext()
	{
		if (PreCheck != null)
		{
			PreCheck();
		}
		while (PandoraSingleton<MissionManager>.Instance.IsNavmeshUpdating)
		{
			yield return 0;
		}
		ABPath aBPath = ABPath.Construct(unitCtrlr.transform.position, targets[pathIdx].position);
		aBPath.calculatePartial = true;
		PandoraSingleton<MissionManager>.Instance.PathSeeker.StartPath(aBPath, OnPathFinish);
	}

	private void OnPathFinish(Path p)
	{
		bool flag = false;
		calculatedPath = p;
		if (p.vectorPath.Count > 0 && ((ABPath)p).endNode == AstarPath.active.GetNearest(targets[pathIdx].position).node)
		{
			float totalLength = p.GetTotalLength();
			flag = (totalLength < maxDist);
			if ((!keepOnlyReachable || (keepOnlyReachable && flag)) && (currentPath == null || (bestPathType == BestPathType.CLOSEST && totalLength < currentPathLg) || (bestPathType == BestPathType.FAREST && totalLength > currentPathLg)))
			{
				bestTarget = targets[pathIdx];
				currentPath = p;
				currentPathLg = totalLength;
				PathFound();
			}
		}
		if (PostCheck != null)
		{
			PostCheck(flag);
		}
		pathIdx++;
		if (pathIdx < targets.Count)
		{
			unitCtrlr.StopCoroutine(CheckNext());
			unitCtrlr.StartCoroutine(CheckNext());
			return;
		}
		PandoraDebug.LogInfo("All paths checked!", "PATHFINDING");
		if (currentPath != null)
		{
			bool num = currentPathLg <= maxDist;
			if (!num && CannotReach != null)
			{
				PandoraDebug.LogInfo("Cannot Reach", "PATHFINDING");
				CannotReach();
			}
			if ((!num && checkDecisionOnCannotReach) || forceDecisionCheck)
			{
				FindPathDecision(bestTarget.position, !forceDecisionCheck);
				return;
			}
			PandoraDebug.LogInfo("Path found successfully!", "PATHFINDING");
			AllChecked(arg0: true);
		}
		else
		{
			if (fallBackToOldPath && !checkDecisionOnCannotReach && !forceDecisionCheck && oldPath != null)
			{
				PandoraDebug.LogInfo("No path found, using old Path", "PATHFINDING");
				currentPath = oldPath;
			}
			else
			{
				PandoraDebug.LogInfo("Path found failed!", "PATHFINDING");
			}
			AllChecked(currentPath != null);
		}
	}

	public void UsedSkill(SkillId skillId, UnitController target = null)
	{
		if (!skillTargets.ContainsKey(skillId))
		{
			skillTargets.Add(skillId, new List<UnitController>());
		}
		if (target != null && skillTargets[skillId].IndexOf(target) == -1)
		{
			skillTargets[skillId].Add(target);
		}
		if (usedSkillTurn.IndexOf(skillId, SkillIdComparer.Instance) == -1)
		{
			usedSkillTurn.Add(skillId);
		}
	}

	public ActionStatus GetBestAction(List<UnitActionId> actionIds, out UnitController target, UnityAction<ActionStatus, List<UnitController>> RefineTargets)
	{
		defendersCopy.Clear();
		defendersCopy.AddRange(unitCtrlr.defenders);
		UnitController unitController = targetEnemy;
		ActionStatus result = null;
		target = null;
		allActions.Clear();
		allTargets.Clear();
		validActions.Clear();
		validTargets.Clear();
		unitCtrlr.UpdateActionStatus(notice: false, UnitActionRefreshId.ALWAYS);
		for (int i = 0; i < unitCtrlr.actionStatus.Count; i++)
		{
			if (actionIds.IndexOf(unitCtrlr.actionStatus[i].ActionId, UnitActionIdComparer.Instance) == -1 || !unitCtrlr.actionStatus[i].Available)
			{
				continue;
			}
			List<UnitController> list = GetTargets(unitCtrlr.actionStatus[i]);
			RefineTargets(unitCtrlr.actionStatus[i], list);
			for (int j = 0; j < list.Count; j++)
			{
				unitCtrlr.defenderCtrlr = list[j];
				unitCtrlr.SetCurrentAction(unitCtrlr.actionStatus[i].SkillId);
				targetEnemy = list[j];
				AiFilterResultId aiFilterResultId = CheckFilters(unitCtrlr.actionStatus[i]);
				PandoraDebug.LogInfo("Skill " + unitCtrlr.actionStatus[i].SkillId + " filter result = " + aiFilterResultId, "AI");
				switch (aiFilterResultId)
				{
				case AiFilterResultId.NONE:
					allActions.Add(unitCtrlr.actionStatus[i]);
					allTargets.Add(list[j]);
					break;
				case AiFilterResultId.VALID:
					validActions.Add(unitCtrlr.actionStatus[i]);
					validTargets.Add(list[j]);
					break;
				}
			}
		}
		if (validActions.Count > 0)
		{
			int index = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, validActions.Count);
			result = validActions[index];
			target = validTargets[index];
		}
		else if (allActions.Count > 0)
		{
			int index2 = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, allActions.Count);
			result = allActions[index2];
			target = allTargets[index2];
		}
		unitCtrlr.defenders.Clear();
		unitCtrlr.defenders.AddRange(defendersCopy);
		targetEnemy = unitController;
		return result;
	}

	private List<UnitController> GetTargets(ActionStatus action)
	{
		tmpTargets.Clear();
		tmpTargets.AddRange(action.Targets);
		float @float = Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC);
		switch (action.TargetingId)
		{
		case TargetingId.LINE:
		case TargetingId.CONE:
		case TargetingId.AREA:
		case TargetingId.AREA_GROUND:
		{
			for (int num = tmpTargets.Count - 1; num >= 0; num--)
			{
				if (!unitCtrlr.IsInRange(tmpTargets[num], action.RangeMin, action.RangeMax + action.Radius, @float, unitBlocking: false, checkAllBones: false, BoneId.NONE))
				{
					tmpTargets.RemoveAt(num);
				}
			}
			break;
		}
		case TargetingId.ARC:
			tmpTargets.AddRange(unitCtrlr.EngagedUnits);
			break;
		}
		return tmpTargets;
	}

	private AiFilterResultId CheckFilters(ActionStatus action)
	{
		List<SkillAiFilterData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillAiFilterData>("fk_skill_id", ((int)action.skillData.Id).ToConstantString());
		excludeFilters.Clear();
		validFilters.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			switch (list[i].AiFilterResultId)
			{
			case AiFilterResultId.EXCLUDED:
				excludeFilters.Add(list[i]);
				break;
			case AiFilterResultId.VALID:
				validFilters.Add(list[i]);
				break;
			}
		}
		action.GetRoll();
		if (CheckFilterType(excludeFilters, AiFilterResultId.EXCLUDED))
		{
			return AiFilterResultId.EXCLUDED;
		}
		if (validFilters.Count == 0 || CheckFilterType(validFilters, AiFilterResultId.VALID))
		{
			return AiFilterResultId.VALID;
		}
		return AiFilterResultId.NONE;
	}

	private bool CheckFilterType(List<SkillAiFilterData> filtersData, AiFilterResultId resultId)
	{
		List<List<SkillAiFilterData>> list = new List<List<SkillAiFilterData>>();
		while (filtersData.Count > 0)
		{
			SkillAiFilterData skillAiFilterData = GetHeadFilter(filtersData[0], filtersData);
			List<SkillAiFilterData> list2 = new List<SkillAiFilterData>();
			list2.Add(skillAiFilterData);
			filtersData.Remove(skillAiFilterData);
			while (skillAiFilterData.SkillAiFilterIdAnd != 0)
			{
				skillAiFilterData = GetFilter(skillAiFilterData.SkillAiFilterIdAnd, filtersData);
				list2.Add(skillAiFilterData);
				filtersData.Remove(skillAiFilterData);
			}
			list.Add(list2);
		}
		for (int i = 0; i < list.Count; i++)
		{
			bool flag = true;
			int num = 0;
			while (flag && num < list[i].Count)
			{
				flag &= IsFilterValid(list[i][num]);
				num++;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private SkillAiFilterData GetHeadFilter(SkillAiFilterData data, List<SkillAiFilterData> filters)
	{
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int i = 0; i < filters.Count; i++)
			{
				if (filters[i].SkillAiFilterIdAnd == data.Id)
				{
					data = filters[i];
					flag = true;
				}
			}
		}
		return data;
	}

	private SkillAiFilterData GetFilter(SkillAiFilterId filterId, List<SkillAiFilterData> filters)
	{
		for (int i = 0; i < filters.Count; i++)
		{
			if (filters[i].Id == filterId)
			{
				return filters[i];
			}
		}
		return null;
	}

	private bool IsFilterValid(SkillAiFilterData filterData)
	{
		UnitController unitController = unitCtrlr;
		if (filterData.CheckTargetInstead)
		{
			unitController = unitCtrlr.AICtrlr.targetEnemy;
		}
		if (unitController == null)
		{
			PandoraDebug.LogWarning("No target found when evaluating skill filter " + filterData.Name, "AI");
			return false;
		}
		bool flag = true;
		bool reverse = filterData.Reverse;
		if (filterData.AttributeId != 0)
		{
			int attributeVal = GetAttributeVal(filterData.AttributeId, unitController);
			int checkValue = filterData.CheckValue;
			if (filterData.AttributeIdCheck != 0)
			{
				checkValue = GetAttributeVal(filterData.AttributeIdCheck, unitController);
			}
			flag &= CheckValue(filterData.AiFilterCheckId, checkValue, attributeVal);
		}
		if (flag && filterData.AiFilterCheckIdEngaged != 0)
		{
			flag &= CheckValue(filterData.AiFilterCheckIdEngaged, filterData.EngagedValue, unitController.EngagedUnits.Count);
		}
		if (flag && filterData.HasAltSet)
		{
			flag &= ((!reverse && unitController.CanSwitchWeapon()) || (reverse && !unitController.CanSwitchWeapon()));
		}
		if (flag && filterData.NeverUsedOnTarget)
		{
			flag = ((PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)filterData.SkillId).TargetingId != TargetingId.SINGLE_TARGET) ? (flag & ((!reverse && !skillTargets.ContainsKey(filterData.SkillId)) || (reverse && skillTargets.ContainsKey(filterData.SkillId)))) : ((!skillTargets.TryGetValue(filterData.SkillId, out List<UnitController> value)) ? (flag && !reverse) : (flag & ((!reverse && value.IndexOf(unitController) == -1) || (reverse && value.IndexOf(unitController) != -1)))));
		}
		if (flag && filterData.NeverUsedTurn)
		{
			flag &= ((!reverse && usedSkillTurn.IndexOf(filterData.SkillId, SkillIdComparer.Instance) == -1) || (reverse && usedSkillTurn.IndexOf(filterData.SkillId, SkillIdComparer.Instance) != -1));
		}
		if (flag && filterData.HasRangeWeapon)
		{
			flag &= ((!reverse && unitController.HasRange()) || (reverse && !unitController.HasRange()));
		}
		if (flag && filterData.IsAllAlone)
		{
			flag &= ((!reverse && unitController.IsAllAlone()) || (reverse && !unitController.IsAllAlone()));
		}
		if (flag && filterData.IsSister)
		{
			flag &= ((!reverse && unitController.GetWarband().WarData.Id == WarbandId.SISTERS_OF_SIGMAR) || (reverse && unitController.GetWarband().WarData.Id != WarbandId.SISTERS_OF_SIGMAR));
		}
		if (flag && filterData.IsStunned)
		{
			flag &= ((!reverse && unitController.unit.Status == UnitStateId.STUNNED) || (reverse && unitController.unit.Status != UnitStateId.STUNNED));
		}
		if (flag && filterData.CannotParry)
		{
			flag &= ((!reverse && !unitController.unit.HasEnchantment(EnchantmentId.ITEM_PARRY)) || (reverse && unitController.unit.HasEnchantment(EnchantmentId.ITEM_PARRY)));
		}
		if (flag && filterData.HasSpell)
		{
			flag &= ((!reverse && unitController.HasSpells()) || (reverse && !unitController.HasSpells()));
		}
		if (flag && filterData.HealthUnderRatio != 0)
		{
			int num = (int)((float)unitController.unit.CurrentWound / (float)unitController.unit.Wound * 100f);
			flag &= ((!reverse && num <= filterData.HealthUnderRatio) || (reverse && num > filterData.HealthUnderRatio));
		}
		if (flag && filterData.MinRoll != 0)
		{
			flag &= ((!reverse && unitCtrlr.CurrentAction.GetRoll() >= filterData.MinRoll) || (reverse && unitCtrlr.CurrentAction.GetRoll() < filterData.MinRoll));
		}
		if (flag && filterData.HasBeenShot)
		{
			flag &= ((!reverse && unitController.beenShot) || (reverse && !unitController.beenShot));
		}
		if (flag && filterData.NoEnemyInSight)
		{
			flag &= ((!reverse && !unitController.HasEnemyInSight()) || (reverse && unitController.HasEnemyInSight()));
		}
		if (flag && filterData.IsPreFight)
		{
			flag &= ((!reverse && preFight) || (reverse && !preFight));
		}
		if (flag && filterData.EnchantmentTypeIdApplied != 0)
		{
			flag &= ((!reverse && unitController.unit.HasEnchantment(filterData.EnchantmentTypeIdApplied)) || (reverse && !unitController.unit.HasEnchantment(filterData.EnchantmentTypeIdApplied)));
		}
		PandoraDebug.LogInfo("Checking filter " + filterData.Name + " using " + unitController.name + " as target. Result is " + flag, "AI");
		return flag;
	}

	private int GetAttributeVal(AttributeId attrId, UnitController target)
	{
		switch (attrId)
		{
		case AttributeId.COMBAT_MELEE_HIT_ROLL:
			return target.GetMeleeHitRoll();
		case AttributeId.COMBAT_RANGE_HIT_ROLL:
			return target.GetRangeHitRoll();
		default:
			return target.unit.GetAttribute(attrId);
		}
	}

	private static bool CheckValue(AiFilterCheckId aiFilterCheckId, int checkValue, int attributeVal)
	{
		switch (aiFilterCheckId)
		{
		case AiFilterCheckId.EQUAL:
			return attributeVal == checkValue;
		case AiFilterCheckId.GREATER:
			return attributeVal > checkValue;
		case AiFilterCheckId.LOWER:
			return attributeVal < checkValue;
		case AiFilterCheckId.GREATER_EQUAL:
			return attributeVal >= checkValue;
		case AiFilterCheckId.LOWER_EQUAL:
			return attributeVal <= checkValue;
		case AiFilterCheckId.NOT_EQUAL:
			return attributeVal != checkValue;
		default:
			return false;
		}
	}
}
