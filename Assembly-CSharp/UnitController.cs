using Prometheus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class UnitController : UnitMenuController, IMyrtilus
{
	public enum State
	{
		NONE,
		TURN_START,
		TURN_MESSAGE,
		UPDATE_EFFECTS,
		RECOVERY,
		TERROR,
		PERSONAL_ROUT,
		FEAR_CHECK,
		STUPIDITY,
		IDLE,
		START_MOVE,
		MOVE,
		ENGAGED,
		PERCEPTION,
		SEARCH,
		INVENTORY,
		ACTIVATE,
		TRAPPED,
		TELEPORT,
		RELOAD,
		DISENGAGE,
		SWITCH_WEAPON,
		DELAY,
		INTERACTIVE_TARGET,
		SINGLE_TARGETING,
		AOE_TARGETING,
		CONE_TARGETING,
		LINE_TARGETING,
		SPELL_CASTING,
		SPELL_CURSE,
		SKILL_USE,
		RANGE_COMBAT_FIRE,
		CLOSE_COMBAT_ATTACK,
		ACTION_WAIT,
		COUNTER_CHOICE,
		ACTIVATE_STANCE,
		OVERWATCH,
		AMBUSH,
		CHARGE,
		TURN_FINISHED,
		FLEE,
		FLEE_MOVE,
		AI_CONTROLLED,
		NET_CONTROLLED,
		OVERVIEW,
		ATHLETIC_COUNTER,
		PREPARE_ATHLETIC,
		ATHLETIC,
		FLY,
		PREPARE_FLY,
		ARC_TARGETING,
		NB_STATE
	}

	public enum CounterChoiceId
	{
		NONE,
		COUNTER,
		NO_COUNTER
	}

	[HideInInspector]
	public enum CommandList
	{
		NONE,
		NETWORK_SYNC,
		SKILL,
		SKILL_SINGLE_TARGET,
		SKILL_MULTIPLE_TARGETS,
		OVERWATCH,
		AMBUSH,
		INVENTORY_CHANGE,
		INVENTORY_DONE,
		INTERACTIVE,
		ACTION_DONE,
		ENGAGED,
		START_MOVE,
		TRAPPED,
		INVENTORY_TAKE_ALL,
		ZONE_AOE,
		CURSE,
		ASK_INTERRUPTION,
		MOVE_AND_UPDATE_CIRCLE,
		ATHLETIC,
		ATHLETIC_FINISHED,
		FLY,
		ENGAGED_UNITS,
		SKILL_SINGLE_DESTRUCTIBLE,
		COUNT
	}

	private struct Command
	{
		public bool reliable;

		public Hermes.SendTarget target;

		public ulong from;

		public uint command;

		public object[] parms;
	}

	public const float FACE_TARGET_TIME = 10f;

	public const float FACE_TARGET_SPEED = 100f;

	public const float TARGET_HEIGHT = 1.5f;

	public const float TARGET_HEIGHT_TARGET = 1.25f;

	public const float MAX_ENGAGED_HEIGHT = 1.9f;

	private const float MAX_CHARGE_HEIGHT = 2.5f;

	private const float CHARGE_COLLISION_HEIGHT_THRESHOLD = 0.5f;

	private const float COMBAT_CIRCLE_FADE_DIST = 80f;

	private const float COMBAT_IDLE_LERP_SPEED = 2f;

	private Color DETECTED_COLOR = new Color(14f / 15f, 1f, 101f / 255f);

	private Color TRAIL_BASE_COLOR = new Color(255f, 255f, 255f, 128f);

	[HideInInspector]
	public State nextState;

	private State actionDoneNextState;

	private Vector3 fixPosition;

	private Rigidbody rigbody;

	public bool isInLadder = true;

	private bool killed;

	public List<ActionStatus> actionStatus;

	public List<ActionStatus> availableActionStatus = new List<ActionStatus>();

	private ActionStatus currentAction;

	[HideInInspector]
	public List<UnitController> friendlyEntered = new List<UnitController>();

	public Vector3 friendlyZoneEntryPoint;

	[HideInInspector]
	public int lastActionWounds;

	[HideInInspector]
	public ActionData currentActionData = new ActionData();

	[HideInInspector]
	public string currentActionLabel;

	[HideInInspector]
	public AttributeId currentAttributeRoll;

	[HideInInspector]
	public string flyingLabel;

	[HideInInspector]
	public string actionOutcomeLabel;

	[HideInInspector]
	public bool ladderVisible;

	private readonly List<FlyingTarget> flyingOverviews = new List<FlyingTarget>();

	[HideInInspector]
	public List<InteractivePoint> interactivePoints = new List<InteractivePoint>();

	[HideInInspector]
	public List<Destructible> triggeredDestructibles = new List<Destructible>();

	[HideInInspector]
	public InteractivePoint interactivePoint;

	[HideInInspector]
	public InteractiveTarget prevInteractiveTarget;

	[HideInInspector]
	public InteractiveTarget nextInteractiveTarget;

	[HideInInspector]
	public ActionDestination activeActionDest;

	[HideInInspector]
	public Transform failedTarget;

	[HideInInspector]
	public TriggerPoint activeTrigger;

	[HideInInspector]
	public Teleporter currentTeleporter;

	[HideInInspector]
	public ZoneAoe currentZoneAoe;

	[HideInInspector]
	public UnitController attackerCtrlr;

	[HideInInspector]
	public List<UnitController> defenders;

	[HideInInspector]
	public List<Destructible> destructTargets = new List<Destructible>();

	[HideInInspector]
	public AttackResultId attackResultId;

	[HideInInspector]
	public EffectTypeId buffResultId;

	[HideInInspector]
	public bool criticalHit;

	[HideInInspector]
	public int attackUsed;

	[HideInInspector]
	public bool beenShot;

	private bool wasMaxWound;

	[HideInInspector]
	public bool wasEngaged;

	[HideInInspector]
	public DynamicCombatCircle combatCircle;

	[HideInInspector]
	public DynamicChargeCircle chargeCircle;

	[HideInInspector]
	public List<UnitController> chargeTargets;

	[HideInInspector]
	public OlympusFire chargeFx;

	[HideInInspector]
	public List<UnitController> chargePreviousTargets;

	private RaycastHit raycastHitInfo;

	[HideInInspector]
	public List<TargetData> targetsData = new List<TargetData>();

	public List<BoneTarget> boneTargets;

	[HideInInspector]
	public Transform hitPoint;

	[HideInInspector]
	public Vector3 startPosition;

	[HideInInspector]
	public Quaternion startRotation;

	[HideInInspector]
	public int searchVariation;

	[HideInInspector]
	public SearchPoint lootBagPoint;

	public List<Item> oldItems;

	[HideInInspector]
	public Vector3 currentSpellTargetPosition;

	[HideInInspector]
	public SpellTypeId currentSpellTypeId;

	[HideInInspector]
	public SkillId currentSpellId;

	[HideInInspector]
	public bool currentSpellSuccess;

	[HideInInspector]
	public SkillId currentCurseSkillId;

	[HideInInspector]
	public int wyrdstoneRollModifier;

	[HideInInspector]
	public float fleeDistanceMultiplier;

	[HideInInspector]
	public float lastTimer;

	[HideInInspector]
	public bool isCaptainMorganing;

	private Queue<Command> commands;

	private bool commandSent;

	private Queue<Command> commandsToSend = new Queue<Command>();

	private bool hasBeenSpotted;

	[HideInInspector]
	public List<UnitController> detectedUnits;

	[HideInInspector]
	public List<TriggerPoint> detectedTriggers;

	[HideInInspector]
	public List<InteractivePoint> detectedInteractivePoints;

	[HideInInspector]
	public int recoveryTarget;

	[HideInInspector]
	public int[] MVUptsPerCategory = new int[5];

	public Quaternion newRotation = Quaternion.identity;

	[HideInInspector]
	public List<SearchPoint> linkedSearchPoints;

	[HideInInspector]
	public bool unlockSearchPointOnDeath;

	[HideInInspector]
	public bool reviveUntilSearchEmpty;

	[HideInInspector]
	public List<DecisionPoint> forcedSpawnPoints;

	[HideInInspector]
	public bool spawnedOnDeath;

	public int hitMod;

	private Quaternion faceTargetRotation;

	private bool hadTerror;

	private bool hadFear;

	private bool defHadTerror;

	private bool defHadFear;

	private int lastChargeMvt = -1;

	private List<Collider> chargeValidColliders = new List<Collider>();

	private float currentAnimSpeed;

	private List<UnitController> newEngagedUnits = new List<UnitController>();

	private List<UnitController> modifiedUnits = new List<UnitController>();

	private List<UnitController> involvedUnits = new List<UnitController>();

	private List<MonoBehaviour> availableTargets = new List<MonoBehaviour>();

	private Vector3 yieldedPos;

	private Quaternion yieldedRot;

	private SkillId yieldedSkillId;

	public CheapStateMachine StateMachine
	{
		get;
		private set;
	}

	public bool Initialized
	{
		get;
		private set;
	}

	public bool IsFixed
	{
		get;
		private set;
	}

	public float CapsuleHeight
	{
		get;
		private set;
	}

	public float CapsuleRadius
	{
		get;
		private set;
	}

	public ActionStatus LastActivatedAction
	{
		get;
		set;
	}

	public ActionStatus CurrentAction
	{
		get
		{
			return currentAction;
		}
		private set
		{
			currentAction = value;
			unit.SetActiveSkill((CurrentAction == null) ? null : CurrentAction.skillData);
		}
	}

	public bool IsInFriendlyZone => friendlyEntered.Count > 0;

	public AIController AICtrlr
	{
		get;
		private set;
	}

	public UnitController defenderCtrlr
	{
		get
		{
			return (defenders == null || defenders.Count <= 0) ? null : defenders[0];
		}
		set
		{
			defenders.Clear();
			if (value != null)
			{
				defenders.Add(value);
			}
		}
	}

	public Destructible destructibleTarget
	{
		get
		{
			return (destructTargets.Count <= 0) ? null : destructTargets[0];
		}
		set
		{
			destructTargets.Clear();
			if (value != null)
			{
				destructTargets.Add(value);
			}
		}
	}

	public bool Sheated
	{
		get;
		private set;
	}

	public bool IsCharging => CurrentAction != null && (CurrentAction.ActionId == UnitActionId.CHARGE || CurrentAction.ActionId == UnitActionId.AMBUSH);

	public List<UnitController> EngagedUnits
	{
		get;
		private set;
	}

	public bool Engaged => EngagedUnits.Count > 0;

	public AttributeModList CurrentRollModifiers
	{
		get;
		private set;
	}

	public AttributeModList CurrentDamageModifiers
	{
		get;
		private set;
	}

	public Beacon CurrentBeacon
	{
		get;
		private set;
	}

	public bool Fleeing
	{
		get;
		set;
	}

	public bool TurnStarted
	{
		get;
		set;
	}

	public bool Resurected
	{
		get;
		private set;
	}

	public Vector3 FleeTarget
	{
		get;
		private set;
	}

	public MapImprint Imprint
	{
		get;
		private set;
	}

	public bool HasBeenSpotted
	{
		get
		{
			if (!hasBeenSpotted)
			{
				hasBeenSpotted = IsImprintVisible();
			}
			return hasBeenSpotted;
		}
		set
		{
			hasBeenSpotted = value;
		}
	}

	public uint uid
	{
		get;
		set;
	}

	public uint owner
	{
		get;
		set;
	}

	protected override void Awake()
	{
		base.Awake();
		StateMachine = new CheapStateMachine(51);
		StateMachine.AddState(new Idle(this), 9);
		StateMachine.AddState(new TurnStart(this), 1);
		StateMachine.AddState(new TurnMessage(this), 2);
		StateMachine.AddState(new UpdateEffects(this), 3);
		StateMachine.AddState(new Recovery(this), 4);
		StateMachine.AddState(new Terror(this), 5);
		StateMachine.AddState(new PersonalRout(this), 6);
		StateMachine.AddState(new FearCheck(this), 7);
		StateMachine.AddState(new Stupidity(this), 8);
		StateMachine.AddState(new StartMove(this), 10);
		StateMachine.AddState(new Moving(this), 11);
		StateMachine.AddState(new Engaged(this), 12);
		StateMachine.AddState(new SwitchWeapon(this), 21);
		StateMachine.AddState(new Reload(this), 19);
		StateMachine.AddState(new Disengage(this), 20);
		StateMachine.AddState(new Delay(this), 22);
		StateMachine.AddState(new Perception(this), 13);
		StateMachine.AddState(new Search(this), 14);
		StateMachine.AddState(new Inventory(this), 15);
		StateMachine.AddState(new Activate(this), 16);
		StateMachine.AddState(new Trapped(this), 17);
		StateMachine.AddState(new Teleport(this), 18);
		StateMachine.AddState(new InteractivePointTarget(this), 23);
		StateMachine.AddState(new SingleTargeting(this), 24);
		StateMachine.AddState(new AOETargeting(this), 25);
		StateMachine.AddState(new ConeTargeting(this), 26);
		StateMachine.AddState(new LineTargeting(this), 27);
		StateMachine.AddState(new SpellCasting(this), 28);
		StateMachine.AddState(new SpellCurse(this), 29);
		StateMachine.AddState(new SkillUse(this), 30);
		StateMachine.AddState(new RangeCombatFire(this), 31);
		StateMachine.AddState(new CloseCombatAttack(this), 32);
		StateMachine.AddState(new ActionWait(this), 33);
		StateMachine.AddState(new CounterChoice(this), 34);
		StateMachine.AddState(new ActivateStance(this), 35);
		StateMachine.AddState(new Overwatch(this), 36);
		StateMachine.AddState(new Ambush(this), 37);
		StateMachine.AddState(new Charge(this), 38);
		StateMachine.AddState(new TurnFinished(this), 39);
		StateMachine.AddState(new Flee(this), 40);
		StateMachine.AddState(new FleeMove(this), 41);
		StateMachine.AddState(new AIControlled(this), 42);
		StateMachine.AddState(new NetControlled(this), 43);
		StateMachine.AddState(new Overview(this), 44);
		StateMachine.AddState(new AthleticCounter(this), 45);
		StateMachine.AddState(new Athletic(this), 47);
		StateMachine.AddState(new PrepareAthletic(this), 46);
		StateMachine.AddState(new Fly(this), 48);
		StateMachine.AddState(new PrepareFly(this), 49);
		StateMachine.AddState(new ArcTargeting(this), 50);
		EngagedUnits = new List<UnitController>();
		defenders = new List<UnitController>();
		chargeTargets = new List<UnitController>();
		chargePreviousTargets = new List<UnitController>();
		CurrentRollModifiers = new AttributeModList();
		CurrentDamageModifiers = new AttributeModList();
		detectedUnits = new List<UnitController>();
		detectedTriggers = new List<TriggerPoint>();
		detectedInteractivePoints = new List<InteractivePoint>();
		commands = new Queue<Command>();
		oldItems = new List<Item>();
		isCaptainMorganing = false;
		Initialized = false;
		IsFixed = false;
		TurnStarted = false;
		lootBagPoint = null;
		hasBeenSpotted = false;
		rigbody = GetComponent<Rigidbody>();
		CapsuleCollider capsuleCollider = (CapsuleCollider)GetComponent<Collider>();
		CapsuleHeight = capsuleCollider.height;
		CapsuleRadius = capsuleCollider.radius;
	}

	private void Start()
	{
		base.Highlight.seeThrough = false;
	}

	public void FirstSyncInit(UnitSave save, uint guid, int warbandIdx, int playerIdx, PlayerTypeId playerTypeId, int idxInWarband, bool merge, bool loadBodyParts = true)
	{
		uid = guid;
		RegisterToHermes();
		owner = (uint)playerIdx;
		if (PandoraSingleton<MissionStartData>.Instance.isReload)
		{
			for (int i = 0; i < PandoraSingleton<MissionStartData>.Instance.units.Count; i++)
			{
				if (PandoraSingleton<MissionStartData>.Instance.units[i].myrtilusId == uid)
				{
					save.items = PandoraSingleton<MissionStartData>.Instance.units[i].unitSave.items;
					break;
				}
			}
		}
		unit = new Unit(save);
		unit.warbandIdx = warbandIdx;
		unit.warbandPos = idxInWarband;
		if (PandoraSingleton<MissionStartData>.Instance.isReload)
		{
			List<MissionEndUnitSave> units = PandoraSingleton<MissionStartData>.Instance.units;
			for (int j = 0; j < units.Count; j++)
			{
				if (uid == units[j].myrtilusId && units[j].status == UnitStateId.OUT_OF_ACTION)
				{
					unit.Status = UnitStateId.OUT_OF_ACTION;
					break;
				}
			}
		}
		InitializeBones();
		Imprint = GetComponent<MapImprint>();
		if (Imprint == null)
		{
			Imprint = base.gameObject.AddComponent<MapImprint>();
		}
		bool flag = IsPlayed();
		Imprint.Init("unit/" + unit.Id.ToLowerString(), "unit/" + UnitId.NONE.ToLowerString(), alwaysVisible: false, MapImprintType.UNIT, Hide, this);
	}

	public void InitMissionUnit(UnitSave save, uint uId, int warbandIdx, int playerIdx, PlayerTypeId playerTypeId, int idxInWarband, bool loadBodyParts)
	{
		if (playerTypeId == PlayerTypeId.AI || playerTypeId == PlayerTypeId.PASSIVE_AI)
		{
			AICtrlr = new AIController(this);
			unit.isAI = true;
		}
		InstantiateAllEquipment();
		if (loadBodyParts)
		{
			LaunchBodyPartsLoading(MissionBodyPartsLoaded, noLOD: false);
		}
		else
		{
			MissionBodyPartsLoaded();
		}
	}

	public void MissionBodyPartsLoaded()
	{
		MergeNoAtlas();
		MissionInitPostProcess();
	}

	public void MissionInitPostProcess()
	{
		foreach (BodyPart value in unit.bodyParts.Values)
		{
			for (int i = 0; i < value.relatedGO.Count; i++)
			{
				value.relatedGO[i].SetActive(value: true);
			}
		}
		InitCloth();
		InitBodyTrails();
		Hide(hide: false, force: true);
		startPosition = base.transform.position;
		InitActionStatus();
		if (combatCircle == null)
		{
			combatCircle = GetComponentInChildren<DynamicCombatCircle>();
		}
		if (combatCircle == null)
		{
			PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_combat_line.prefab", delegate(UnityEngine.Object go)
			{
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(go);
				gameObject2.transform.SetParent(base.transform);
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				combatCircle = gameObject2.GetComponent<DynamicCombatCircle>();
				combatCircle.gameObject.SetActive(value: true);
				combatCircle.Init();
			});
		}
		else
		{
			combatCircle.gameObject.SetActive(value: true);
			combatCircle.Init();
		}
		if (chargeCircle == null)
		{
			chargeCircle = GetComponentInChildren<DynamicChargeCircle>();
		}
		if (chargeCircle == null)
		{
			PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_charge_line.prefab", delegate(UnityEngine.Object go)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(go);
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				chargeCircle = gameObject.GetComponent<DynamicChargeCircle>();
				chargeCircle.gameObject.SetActive(value: false);
				chargeCircle.Init();
			});
		}
		else
		{
			chargeCircle.gameObject.SetActive(value: false);
			chargeCircle.Init();
		}
		fleeDistanceMultiplier = Constant.GetFloat(ConstantId.FLEE_MOVEMENT_MULTIPLIER);
		AnimatorCullingMode cullingMode = animator.cullingMode;
		animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		animator.Play(AnimatorIds.idle, -1);
		animator.cullingMode = cullingMode;
		animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
		animator.applyRootMotion = true;
		InitBoneTargets();
		if (shaderSetter != null)
		{
			shaderSetter.ApplyShaderParams();
		}
		UnityEngine.Object.Destroy(shaderSetter);
		StateMachine.ChangeState(9);
		Initialized = true;
		if (finishedLoad != null)
		{
			finishedLoad();
		}
	}

	public void StartGameInitialization()
	{
		unit.ResetPoints();
		unit.SetAttribute(AttributeId.CURRENT_OFFENSE_POINTS, Mathf.Min(unit.CurrentOffensePoints, GetWarband().IsAmbushed() ? 1 : 2));
		if (!PandoraSingleton<MissionStartData>.Instance.isReload)
		{
			unit.AddEnchantment(EnchantmentId.BASE_SKILL_STANCE_DODGE, unit, original: false, updateAttributes: false);
			if (GetWarband().IsAmbusher())
			{
				unit.AddEnchantment(EnchantmentId.BALANCING_AMBUSHER_BUFF, unit, original: false, updateAttributes: false);
			}
			else if (GetWarband().IsAmbushed())
			{
				unit.AddEnchantment(EnchantmentId.BALANCING_AMBUSHEE_DEBUFF, unit, original: false, updateAttributes: false);
			}
			if (AICtrlr != null)
			{
				int num = 0;
				for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
				{
					if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].playerTypeId == PlayerTypeId.PLAYER)
					{
						num++;
					}
				}
				if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign && num == 1)
				{
					int num2 = 970;
					num2 += unit.Rank;
					unit.AddEnchantment((EnchantmentId)num2, unit, original: false, updateAttributes: false);
				}
				if (GetWarband().IsRoaming())
				{
					unit.AddEnchantment(PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>(1).EnchantmentId, unit, original: false, updateAttributes: false);
				}
				else
				{
					unit.AddEnchantment(PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.ratingId).EnchantmentId, unit, original: false, updateAttributes: false);
				}
				if (unit.UnitSave.campaignId != 0)
				{
					unit.AddEnchantment(EnchantmentId.PERK_NO_SEARCH, unit, original: false, updateAttributes: false);
				}
			}
			unit.UpdateAttributes();
			unit.SetAttribute(AttributeId.CURRENT_WOUND, unit.Wound);
		}
		unit.UpdateEnchantmentsFx();
		unit.CacheBackpackSize();
		SwitchWeapons(unit.ActiveWeaponSlot);
		InitTargetsData();
	}

	public void Deployed(bool checkEngaged = true)
	{
		Ground();
		SetCombatCircle(this, forced: true);
		animator.Play(AnimatorIds.idle, -1, (float)PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0.0, 1.0));
		if ((UnityEngine.Object)(object)base.Highlight != null)
		{
			base.Highlight.ReinitMaterials();
		}
		if (IsPlayed())
		{
			Imprint.alwaysVisible = true;
		}
		if (linkedSearchPoints != null && unit.Status != UnitStateId.OUT_OF_ACTION && unlockSearchPointOnDeath)
		{
			for (int i = 0; i < linkedSearchPoints.Count; i++)
			{
				linkedSearchPoints[i].gameObject.SetActive(value: false);
			}
		}
		if (PandoraSingleton<MissionStartData>.Instance.isReload)
		{
			List<MissionEndUnitSave> units = PandoraSingleton<MissionStartData>.Instance.units;
			for (int j = 0; j < units.Count; j++)
			{
				if (uid != units[j].myrtilusId)
				{
					continue;
				}
				base.transform.rotation = Quaternion.Euler(units[j].rotation);
				SetFixed(units[j].position, fix: true);
				if (units[j].status == UnitStateId.STUNNED)
				{
					animator.Play(AnimatorIds.kneeling_stunned);
					animator.SetInteger(AnimatorIds.unit_state, 2);
					unit.Status = UnitStateId.STUNNED;
				}
				else if (units[j].status == UnitStateId.OUT_OF_ACTION)
				{
					animator.Play(AnimatorIds.out_of_action);
					animator.SetInteger(AnimatorIds.unit_state, 3);
					unit.Status = UnitStateId.OUT_OF_ACTION;
					GetComponent<Collider>().enabled = false;
					Imprint.alive = false;
					Imprint.needsRefresh = true;
				}
				List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
				for (int k = 0; k < units[j].enchantments.Count; k++)
				{
					EndUnitEnchantment endUnitEnchantment = units[j].enchantments[k];
					UnitController unitController = null;
					for (int l = 0; l < allUnits.Count; l++)
					{
						if (allUnits[l].uid == endUnitEnchantment.ownerMyrtilusId)
						{
							unitController = allUnits[l];
						}
					}
					if (unitController == null)
					{
						List<UnitController> excludedUnits = PandoraSingleton<MissionManager>.Instance.excludedUnits;
						for (int m = 0; m < excludedUnits.Count; m++)
						{
							if (excludedUnits[m].uid == endUnitEnchantment.ownerMyrtilusId)
							{
								unitController = excludedUnits[m];
							}
						}
					}
					Enchantment enchantment = unit.AddEnchantment(endUnitEnchantment.enchantId, unitController.unit, original: false, updateAttributes: true, (AllegianceId)endUnitEnchantment.runeAllegianceId);
					if (enchantment != null)
					{
						enchantment.Duration = endUnitEnchantment.durationLeft;
					}
				}
				if (unit.Status != UnitStateId.OUT_OF_ACTION)
				{
					SwitchWeapons(units[j].weaponSet);
					unit.UpdateAttributes();
					unit.SetAttribute(AttributeId.CURRENT_WOUND, units[j].currentWounds);
					unit.SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, units[j].currentSP);
					unit.SetAttribute(AttributeId.CURRENT_OFFENSE_POINTS, units[j].currentOP);
					TurnStarted = units[j].turnStarted;
				}
				ladderVisible = units[j].isLadderVisible;
				unit.SetAttribute(AttributeId.CURRENT_MVU, units[j].currentMvu);
				MVUptsPerCategory = units[j].mvuPerCategories;
				break;
			}
		}
		SetCombatCircle(this, forced: true);
		if (checkEngaged)
		{
			CheckEngaged(applyEnchants: true);
		}
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(this);
	}

	private void OnDestroy()
	{
		StateMachine.Destroy();
		RemoveFromHermes();
	}

	private void Update()
	{
		commandSent = false;
		if (commandsToSend.Count > 0)
		{
			Command command = commandsToSend.Dequeue();
			Send(command.reliable, command.target, (uint)command.from, command.command, command.parms);
		}
		if (nextState != 0)
		{
			int stateIndex = (int)nextState;
			nextState = State.NONE;
			StateMachine.ChangeState(stateIndex);
		}
		while (commands.Count > 0 && CanLaunchCommand())
		{
			Command com = commands.Dequeue();
			RunCommand(com);
		}
		StateMachine.Update();
		float @float = animator.GetFloat(AnimatorIds.speed);
		if (Engaged && @float > -1f && currentAnimSpeed <= 0f)
		{
			currentAnimSpeed -= Time.deltaTime * 2f;
			currentAnimSpeed = Mathf.Max(currentAnimSpeed, -1f);
			animator.SetFloat(AnimatorIds.speed, currentAnimSpeed);
		}
		if (!Engaged && animator.GetFloat(AnimatorIds.speed) < 0f)
		{
			currentAnimSpeed += Time.deltaTime * 2f;
			currentAnimSpeed = Mathf.Min(currentAnimSpeed, 0f);
			base.SetAnimSpeed(currentAnimSpeed);
		}
	}

	private bool CanLaunchCommand()
	{
		PandoraDebug.LogInfo("Check CanLaunch Command! State = " + (State)StateMachine.GetActiveStateId() + " sequence is playing : " + PandoraSingleton<SequenceManager>.Instance.isPlaying, "UNIT", this);
		return !Fleeing && (!PandoraSingleton<SequenceManager>.Instance.isPlaying || StateMachine.GetActiveStateId() == 38) && (StateMachine.GetActiveStateId() == 9 || StateMachine.GetActiveStateId() == 11 || StateMachine.GetActiveStateId() == 12 || StateMachine.GetActiveStateId() == 14 || StateMachine.GetActiveStateId() == 15 || StateMachine.GetActiveStateId() == 16 || StateMachine.GetActiveStateId() == 17 || StateMachine.GetActiveStateId() == 18 || StateMachine.GetActiveStateId() == 20 || StateMachine.GetActiveStateId() == 23 || StateMachine.GetActiveStateId() == 46 || StateMachine.GetActiveStateId() == 24 || StateMachine.GetActiveStateId() == 25 || StateMachine.GetActiveStateId() == 26 || StateMachine.GetActiveStateId() == 27 || StateMachine.GetActiveStateId() == 33 || StateMachine.GetActiveStateId() == 34 || StateMachine.GetActiveStateId() == 36 || StateMachine.GetActiveStateId() == 37 || StateMachine.GetActiveStateId() == 38 || StateMachine.GetActiveStateId() == 39 || StateMachine.GetActiveStateId() == 40 || StateMachine.GetActiveStateId() == 41 || StateMachine.GetActiveStateId() == 42 || StateMachine.GetActiveStateId() == 43 || StateMachine.GetActiveStateId() == 44 || StateMachine.GetActiveStateId() == 49);
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (IsFixed)
		{
			base.transform.position = fixPosition;
		}
		lastChargeMvt = -1;
		Resurected = false;
	}

	private void FixedUpdate()
	{
		StateMachine.FixedUpdate();
		if (newRotation != Quaternion.identity)
		{
			base.transform.rotation = newRotation;
		}
	}

	public bool IsCurrentState(State state)
	{
		return StateMachine.GetActiveStateId() == (int)state;
	}

	public void SetFixed(bool fix)
	{
		if (fix)
		{
			fixPosition = base.transform.position;
			rigbody.drag = 100f;
		}
		else
		{
			rigbody.drag = 0f;
		}
		IsFixed = fix;
	}

	public void SetFixed(Vector3 position, bool fix)
	{
		base.transform.position = position;
		SetFixed(fix);
		for (int i = 0; i < cloths.Count; i++)
		{
			if (cloths[i] != null)
			{
				cloths[i].ClearTransformMotion();
			}
		}
	}

	public void SetKinemantic(bool kine)
	{
		rigbody.isKinematic = kine;
	}

	public WarbandController GetWarband()
	{
		if (unit.warbandIdx < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count)
		{
			return PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[unit.warbandIdx];
		}
		return null;
	}

	public bool IsMine()
	{
		return owner == PandoraSingleton<Hermes>.Instance.PlayerIndex;
	}

	public bool IsEnemy(UnitController otherUnit)
	{
		WarbandController warband = GetWarband();
		WarbandController warband2 = otherUnit.GetWarband();
		return warband.idx != warband2.idx && warband.teamIdx != warband2.teamIdx && warband2.BlackListed(warband.idx);
	}

	public string GetNameVisible()
	{
		return (!IsImprintVisible() && !IsPlayed()) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("unknown_unit") : unit.Name;
	}

	public string GetLogName()
	{
		bool flag = IsPlayed();
		StringBuilder stringBuilder = PandoraUtils.StringBuilder;
		stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!flag) ? "color_red" : "color_blue"));
		stringBuilder.Append(GetNameVisible());
		stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_end"));
		return stringBuilder.ToString();
	}

	public bool IsPlayed()
	{
		if (GetWarband() != null)
		{
			return GetWarband().IsPlayed();
		}
		return false;
	}

	public bool IsImprintVisible()
	{
		bool flag = Imprint.State == MapImprintStateId.VISIBLE;
		hasBeenSpotted |= flag;
		return flag;
	}

	public int GetCurrentShots()
	{
		if (base.Equipments != null && base.Equipments[(int)unit.ActiveWeaponSlot] != null)
		{
			return base.Equipments[(int)unit.ActiveWeaponSlot].CurrentShots();
		}
		return 0;
	}

	public int GetMaxShots()
	{
		if (base.Equipments != null && base.Equipments[(int)unit.ActiveWeaponSlot] != null)
		{
			return base.Equipments[(int)unit.ActiveWeaponSlot].Item.Shots;
		}
		return 0;
	}

	public void FaceTarget(Transform target, bool force = false)
	{
		if (!(target == base.transform))
		{
			FaceTarget(target.position, force);
		}
	}

	public void FaceTarget(Vector3 position, bool force = false)
	{
		float x = position.x;
		Vector3 position2 = base.transform.position;
		if (Mathf.Approximately(x, position2.x))
		{
			float z = position.z;
			Vector3 position3 = base.transform.position;
			if (Mathf.Approximately(z, position3.z))
			{
				return;
			}
		}
		if (unit.Status == UnitStateId.OUT_OF_ACTION || unit.Id == UnitId.MANTICORE)
		{
			return;
		}
		float x2 = position.x;
		Vector3 position4 = base.transform.position;
		if (Mathf.Abs(x2 - position4.x) < 0.01f)
		{
			float z2 = position.z;
			Vector3 position5 = base.transform.position;
			if (Mathf.Abs(z2 - position5.z) < 0.01f)
			{
				return;
			}
		}
		Quaternion quaternion = default(Quaternion);
		quaternion.SetLookRotation(position - base.transform.position, Vector3.up);
		Vector3 eulerAngles = quaternion.eulerAngles;
		eulerAngles.x = 0f;
		eulerAngles.z = 0f;
		quaternion = Quaternion.Euler(eulerAngles);
		StopCoroutine("Face");
		if (force)
		{
			base.transform.rotation = quaternion;
			return;
		}
		Vector3 eulerAngles2 = base.transform.rotation.eulerAngles;
		float y = eulerAngles2.y;
		Vector3 eulerAngles3 = quaternion.eulerAngles;
		if (y != eulerAngles3.y)
		{
			faceTargetRotation = quaternion;
			StartCoroutine("Face");
		}
	}

	private IEnumerator Face()
	{
		while (true)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, faceTargetRotation, 10f * Time.deltaTime);
			if (Quaternion.Angle(base.transform.rotation, faceTargetRotation) < 0.1f)
			{
				break;
			}
			yield return 0;
		}
	}

	private void OnAnimatorIK()
	{
		if (!isCaptainMorganing || !(base.animator.deltaPosition == Vector3.zero) || unit.Status != 0 || base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash != AnimatorIds.idle)
		{
			return;
		}
		LayerMask mask = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("ground"));
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		Vector3 iKPosition = base.animator.GetIKPosition(AvatarIKGoal.RightFoot);
		float y = iKPosition.y;
		Vector3 bodyPosition = base.animator.bodyPosition;
		float num5 = Mathf.Abs(y - bodyPosition.y);
		Ray ray = new Ray(iKPosition + Vector3.up * 0.4f, -Vector3.up);
		if (Physics.Raycast(ray, out RaycastHit hitInfo, 2f, mask))
		{
			Vector3 point = hitInfo.point;
			float y2 = point.y;
			Vector3 bodyPosition2 = base.animator.bodyPosition;
			num3 = Mathf.Abs(y2 - bodyPosition2.y);
			Vector3 point2 = hitInfo.point;
			point2.y += base.animator.rightFeetBottomHeight;
			num2 = num5 - num3 + base.animator.rightFeetBottomHeight;
			if ((double)Mathf.Abs(num2) < 0.4)
			{
				base.animator.SetIKPosition(AvatarIKGoal.RightFoot, point2);
				base.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
				Quaternion iKRotation = base.animator.GetIKRotation(AvatarIKGoal.RightFoot);
				Vector3 fromDirection = iKRotation * Vector3.up;
				iKRotation = Quaternion.FromToRotation(fromDirection, hitInfo.normal) * iKRotation;
				base.animator.SetIKRotation(AvatarIKGoal.RightFoot, iKRotation);
				base.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
			}
		}
		Vector3 iKPosition2 = base.animator.GetIKPosition(AvatarIKGoal.LeftFoot);
		float y3 = iKPosition2.y;
		Vector3 bodyPosition3 = base.animator.bodyPosition;
		float num6 = Mathf.Abs(y3 - bodyPosition3.y);
		Ray ray2 = new Ray(iKPosition2 + Vector3.up * 0.4f, -Vector3.up);
		if (Physics.Raycast(ray2, out RaycastHit hitInfo2, 2f, mask))
		{
			Vector3 point3 = hitInfo2.point;
			float y4 = point3.y;
			Vector3 bodyPosition4 = base.animator.bodyPosition;
			num4 = Mathf.Abs(y4 - bodyPosition4.y);
			Vector3 point4 = hitInfo2.point;
			point4.y += base.animator.leftFeetBottomHeight;
			num = num6 - num4 + base.animator.leftFeetBottomHeight;
			if ((double)Mathf.Abs(num) < 0.4)
			{
				base.animator.SetIKPosition(AvatarIKGoal.LeftFoot, point4);
				base.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
				Quaternion iKRotation2 = base.animator.GetIKRotation(AvatarIKGoal.LeftFoot);
				Vector3 fromDirection2 = iKRotation2 * Vector3.up;
				iKRotation2 = Quaternion.FromToRotation(fromDirection2, hitInfo2.normal) * iKRotation2;
				base.animator.SetIKRotation(AvatarIKGoal.LeftFoot, iKRotation2);
				base.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
			}
		}
		if (!((double)Mathf.Abs(num) < 0.4) || !((double)Mathf.Abs(num2) < 0.4) || (!((double)num2 > 0.1) && !((double)num2 < -0.1) && !((double)num > 0.1) && !((double)num < -0.1)))
		{
			return;
		}
		if ((double)num2 < -0.1 && (double)num < -0.1)
		{
			if (num2 > num)
			{
				Animator animator = base.animator;
				Vector3 bodyPosition5 = base.animator.bodyPosition;
				float x = bodyPosition5.x;
				Vector3 bodyPosition6 = base.animator.bodyPosition;
				float y5 = bodyPosition6.y + num;
				Vector3 bodyPosition7 = base.animator.bodyPosition;
				animator.bodyPosition = new Vector3(x, y5, bodyPosition7.z);
			}
			else
			{
				Animator animator2 = base.animator;
				Vector3 bodyPosition8 = base.animator.bodyPosition;
				float x2 = bodyPosition8.x;
				Vector3 bodyPosition9 = base.animator.bodyPosition;
				float y6 = bodyPosition9.y + num2;
				Vector3 bodyPosition10 = base.animator.bodyPosition;
				animator2.bodyPosition = new Vector3(x2, y6, bodyPosition10.z);
			}
		}
		else if ((double)num2 > 0.1 && (double)num < -0.1)
		{
			Animator animator3 = base.animator;
			Vector3 bodyPosition11 = base.animator.bodyPosition;
			float x3 = bodyPosition11.x;
			Vector3 bodyPosition12 = base.animator.bodyPosition;
			float y7 = bodyPosition12.y - num2;
			Vector3 bodyPosition13 = base.animator.bodyPosition;
			animator3.bodyPosition = new Vector3(x3, y7, bodyPosition13.z);
		}
		else if ((double)num2 < -0.1 && (double)num > 0.1)
		{
			Animator animator4 = base.animator;
			Vector3 bodyPosition14 = base.animator.bodyPosition;
			float x4 = bodyPosition14.x;
			Vector3 bodyPosition15 = base.animator.bodyPosition;
			float y8 = bodyPosition15.y - num;
			Vector3 bodyPosition16 = base.animator.bodyPosition;
			animator4.bodyPosition = new Vector3(x4, y8, bodyPosition16.z);
		}
		else if ((double)num2 > 0.1 && (double)num > 0.1)
		{
			if (num2 > num)
			{
				Animator animator5 = base.animator;
				Vector3 bodyPosition17 = base.animator.bodyPosition;
				float x5 = bodyPosition17.x;
				Vector3 bodyPosition18 = base.animator.bodyPosition;
				float y9 = bodyPosition18.y + num;
				Vector3 bodyPosition19 = base.animator.bodyPosition;
				animator5.bodyPosition = new Vector3(x5, y9, bodyPosition19.z);
			}
			else
			{
				Animator animator6 = base.animator;
				Vector3 bodyPosition20 = base.animator.bodyPosition;
				float x6 = bodyPosition20.x;
				Vector3 bodyPosition21 = base.animator.bodyPosition;
				float y10 = bodyPosition21.y + num2;
				Vector3 bodyPosition22 = base.animator.bodyPosition;
				animator6.bodyPosition = new Vector3(x6, y10, bodyPosition22.z);
			}
		}
		else if ((double)num2 < -0.1 || (double)num < -0.1)
		{
			if (num2 < num)
			{
				Animator animator7 = base.animator;
				Vector3 bodyPosition23 = base.animator.bodyPosition;
				float x7 = bodyPosition23.x;
				Vector3 bodyPosition24 = base.animator.bodyPosition;
				float y11 = bodyPosition24.y + num2;
				Vector3 bodyPosition25 = base.animator.bodyPosition;
				animator7.bodyPosition = new Vector3(x7, y11, bodyPosition25.z);
			}
			else
			{
				Animator animator8 = base.animator;
				Vector3 bodyPosition26 = base.animator.bodyPosition;
				float x8 = bodyPosition26.x;
				Vector3 bodyPosition27 = base.animator.bodyPosition;
				float y12 = bodyPosition27.y + num;
				Vector3 bodyPosition28 = base.animator.bodyPosition;
				animator8.bodyPosition = new Vector3(x8, y12, bodyPosition28.z);
			}
		}
	}

	public void ResetAtkUsed()
	{
		attackUsed = 0;
	}

	private void InitActionStatus()
	{
		actionStatus = new List<ActionStatus>();
		List<SkillData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>("fk_skill_type_id", 1.ToConstantString());
		for (int i = 0; i < list.Count; i++)
		{
			actionStatus.Add(new ActionStatus(list[i], this));
		}
		for (int j = 0; j < unit.ActiveSkills.Count; j++)
		{
			actionStatus.Add(new ActionStatus(unit.ActiveSkills[j], this));
		}
		for (int k = 0; k < unit.Spells.Count; k++)
		{
			actionStatus.Add(new ActionStatus(unit.Spells[k], this));
		}
	}

	public bool UpdateActionStatus(bool notice, UnitActionRefreshId refreshType = UnitActionRefreshId.NONE)
	{
		if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() == this)
		{
			UpdateTargetsData();
		}
		availableActionStatus.Clear();
		List<Item> list = new List<Item>();
		for (int num = this.actionStatus.Count - 1; num >= 0; num--)
		{
			if (this.actionStatus[num].LinkedItem != null)
			{
				if (!unit.HasItem(this.actionStatus[num].LinkedItem))
				{
					this.actionStatus.RemoveAt(num);
				}
				else
				{
					list.Add(this.actionStatus[num].LinkedItem);
				}
			}
		}
		for (int i = 6; i < unit.Items.Count; i++)
		{
			if (unit.Items[i].Id == ItemId.NONE || unit.Items[i].ConsumableData == null || unit.Items[i].ConsumableData.OutOfCombat)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Id == unit.Items[i].Id && list[j].QualityData.Id == unit.Items[i].QualityData.Id)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.actionStatus.Add(new ActionStatus(unit.Items[i], this));
				list.Add(unit.Items[i]);
			}
		}
		bool flag2 = refreshType != UnitActionRefreshId.ALWAYS;
		for (int k = 0; k < this.actionStatus.Count; k++)
		{
			ActionStatus actionStatus = this.actionStatus[k];
			if (refreshType == UnitActionRefreshId.NONE || refreshType == UnitActionRefreshId.ALWAYS || actionStatus.actionData.UnitActionRefreshId == refreshType)
			{
				bool available = actionStatus.Available;
				actionStatus.UpdateAvailable();
				flag2 |= (available != actionStatus.Available);
			}
			if (actionStatus.Available)
			{
				availableActionStatus.Add(actionStatus);
			}
		}
		availableActionStatus.Sort(new ActionStatusComparer());
		if (!notice || !flag2 || IsPlayed())
		{
		}
		return flag2;
	}

	public void SetCurrentAction(SkillId id)
	{
		CurrentAction = GetAction(id);
		RecalculateModifiers();
	}

	public void RecalculateModifiers()
	{
		CurrentRollModifiers.Clear();
		CurrentDamageModifiers.Clear();
		if (CurrentAction != null && IsPlayed())
		{
			CurrentAction.GetRoll(updateModifiers: true);
			CurrentAction.GetMinDamage(updateModifiers: true);
		}
	}

	public bool CanShowEnchantment(Enchantment x, EffectTypeId effecType)
	{
		return effecType == x.Data.EffectTypeId && !x.Data.NoDisplay && !unit.HasEnchantmentImmunity(x);
	}

	public int GetEffectTypeCount(EffectTypeId effectType)
	{
		int num = 0;
		num += GetEnchantmentsCount(unit.Enchantments, effectType);
		for (int i = 0; i < unit.ActiveItems.Count; i++)
		{
			num += GetEnchantmentsCount(unit.ActiveItems[i].Enchantments, effectType);
		}
		for (int j = 0; j < unit.Injuries.Count; j++)
		{
			num += GetEnchantmentsCount(unit.Injuries[j].Enchantments, effectType);
		}
		for (int k = 0; k < unit.Mutations.Count; k++)
		{
			num += GetEnchantmentsCount(unit.Mutations[k].Enchantments, effectType);
		}
		return num;
	}

	private int GetEnchantmentsCount(List<Enchantment> enchantments, EffectTypeId effectType)
	{
		int num = 0;
		for (int i = 0; i < enchantments.Count; i++)
		{
			if (CanShowEnchantment(enchantments[i], effectType))
			{
				num++;
			}
		}
		return num;
	}

	public ActionStatus GetAction(SkillId skillId)
	{
		for (int i = 0; i < this.actionStatus.Count; i++)
		{
			if (this.actionStatus[i].SkillId == skillId)
			{
				this.actionStatus[i].UpdateAvailable();
				return this.actionStatus[i];
			}
		}
		SkillData data = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)skillId);
		ActionStatus actionStatus = new ActionStatus(data, this);
		actionStatus.UpdateAvailable();
		return actionStatus;
	}

	public List<ActionStatus> GetActions(UnitActionId actionId)
	{
		List<ActionStatus> list = new List<ActionStatus>();
		for (int i = 0; i < actionStatus.Count; i++)
		{
			if (actionStatus[i].ActionId == actionId)
			{
				list.Add(actionStatus[i]);
			}
		}
		return list;
	}

	public void WaitForAction(State nextState)
	{
		PandoraDebug.LogInfo("WaitForAction Unit " + base.name + " state " + nextState, "WAIT FOR ACTION");
		PandoraSingleton<MissionManager>.Instance.delayedUnits.Push(this);
		actionDoneNextState = nextState;
		StateMachine.ChangeState(33);
	}

	public void LaunchMelee(State nextState)
	{
		PandoraDebug.LogDebug("Launch Melee", "ACTION", this);
		if (defenderCtrlr == null)
		{
			defenderCtrlr = EngagedUnits[0];
		}
		hadTerror = unit.HasEnchantment(EnchantmentTypeId.TERROR);
		hadFear = unit.HasEnchantment(EnchantmentTypeId.FEAR);
		defHadTerror = defenderCtrlr.unit.HasEnchantment(EnchantmentTypeId.TERROR);
		defHadFear = defenderCtrlr.unit.HasEnchantment(EnchantmentTypeId.FEAR);
		WaitForAction(nextState);
		TriggerEnchantments(EnchantmentTriggerId.ON_ENGAGE);
		defenderCtrlr.defenderCtrlr = this;
		defenderCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_ENGAGE);
		CheckTerror();
	}

	public void CheckAllAlone()
	{
		if (!unit.HasEnchantment(EnchantmentTypeId.ALL_ALONE) && IsAllAlone())
		{
			unit.AddEnchantment(EnchantmentId.ALL_ALONE, unit, original: false);
		}
		else if (unit.HasEnchantment(EnchantmentTypeId.ALL_ALONE) && !IsAllAlone())
		{
			unit.RemoveEnchantments(EnchantmentTypeId.ALL_ALONE);
		}
		if (unit.HasEnchantment(EnchantmentTypeId.ALL_ALONE))
		{
			nextState = State.PERSONAL_ROUT;
		}
		else
		{
			CheckTerror();
		}
	}

	public bool IsAllAlone()
	{
		if (EngagedUnits.Count > 1)
		{
			int num = 0;
			for (int i = 0; i < EngagedUnits.Count; i++)
			{
				UnitController unitController = EngagedUnits[i];
				if (unitController.unit.Status != 0 || !unitController.HasClose())
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < unitController.EngagedUnits.Count; j++)
				{
					if (unitController.EngagedUnits[j] != this && unitController.EngagedUnits[j].unit.Status == UnitStateId.NONE && unitController.EngagedUnits[j].HasClose())
					{
						flag = true;
					}
				}
				if (!flag)
				{
					num++;
				}
			}
			if (num >= 2)
			{
				return true;
			}
		}
		return false;
	}

	public void ReapplyOnEngage()
	{
		for (int i = 0; i < EngagedUnits.Count; i++)
		{
			UnitController defenderCtrlr = EngagedUnits[i].defenderCtrlr;
			EngagedUnits[i].defenderCtrlr = this;
			EngagedUnits[i].TriggerEnchantments(EnchantmentTriggerId.ON_ENGAGE);
			EngagedUnits[i].defenderCtrlr = defenderCtrlr;
		}
	}

	public void CheckTerror()
	{
		if (unit.HasEnchantment(EnchantmentTypeId.TERROR) && !hadTerror)
		{
			nextState = State.TERROR;
		}
		else if (defenderCtrlr != null && defenderCtrlr.unit.HasEnchantment(EnchantmentTypeId.TERROR) && !defHadTerror)
		{
			defenderCtrlr.StateMachine.ChangeState(5);
			StateMachine.ChangeState(33);
		}
		else
		{
			CheckFear();
		}
		hadTerror = false;
		defHadTerror = false;
	}

	public void CheckFear()
	{
		if (unit.HasEnchantment(EnchantmentTypeId.FEAR) && !hadFear)
		{
			nextState = State.FEAR_CHECK;
		}
		else if (defenderCtrlr != null && defenderCtrlr.unit.HasEnchantment(EnchantmentTypeId.FEAR) && !defHadFear)
		{
			defenderCtrlr.StateMachine.ChangeState(7);
			StateMachine.ChangeState(33);
		}
		else
		{
			ActionDone();
		}
		hadFear = false;
		defHadFear = false;
	}

	public void ActionDone()
	{
		PandoraSingleton<MissionManager>.Instance.TurnTimer.Resume();
		StateMachine.ChangeState(9);
		PandoraDebug.LogDebug("Unit " + base.name + " from warband " + GetWarband().idx + " is calling ActionDone", "WAIT FOR ACTION", this);
		UnitController unitController = PandoraSingleton<MissionManager>.Instance.delayedUnits.Pop();
		PandoraDebug.LogDebug("Action Done: delayed Unit was " + unitController.name + " next state " + unitController.actionDoneNextState, "WAIT FOR ACTION", this);
		unitController.StateMachine.ChangeState((int)unitController.actionDoneNextState);
	}

	public void SetChargeTargets(bool forceSendNotice = false)
	{
		if (unit.ChargeMovement == lastChargeMvt)
		{
			return;
		}
		lastChargeMvt = unit.ChargeMovement;
		List<UnitController> list = chargeTargets;
		chargeTargets = chargePreviousTargets;
		chargePreviousTargets = list;
		chargeTargets.Clear();
		List<UnitController> aliveEnemies = PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(unit.warbandIdx);
		for (int i = 0; i < aliveEnemies.Count; i++)
		{
			if (CanChargeUnit(aliveEnemies[i]))
			{
				chargeTargets.Add(aliveEnemies[i]);
			}
		}
		if (!forceSendNotice && chargeTargets.Count == chargePreviousTargets.Count)
		{
			return;
		}
		bool flag = true;
		for (int j = 0; j < chargeTargets.Count; j++)
		{
			if (!flag)
			{
				break;
			}
			flag = false;
			for (int k = 0; k < chargePreviousTargets.Count; k++)
			{
				if (chargePreviousTargets[k] == chargeTargets[j])
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.COMBAT_HIGHLIGHT_TARGET, -1, chargeTargets);
		}
	}

	public bool CanChargeUnit(UnitController enemy, bool isAmbush = false)
	{
		if (enemy.IsInFriendlyZone)
		{
			return false;
		}
		float @float = Constant.GetFloat((unit.Data.UnitSizeId != UnitSizeId.LARGE && enemy.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? ConstantId.MELEE_RANGE_NORMAL : ConstantId.MELEE_RANGE_LARGE);
		float num = (!isAmbush) ? unit.ChargeMovement : unit.AmbushMovement;
		num += @float;
		float num2 = Vector3.Distance(base.transform.position, enemy.transform.position);
		bool flag = false;
		float num3 = (!isAmbush) ? Constant.GetFloat(ConstantId.CHARGE_MIN_DIST) : @float;
		Vector3 position = base.transform.position;
		float y = position.y;
		Vector3 position2 = enemy.transform.position;
		float num4 = Mathf.Abs(y - position2.y);
		if (num2 >= num3 && num2 <= num && num4 < 2.5f)
		{
			float num5 = CapsuleRadius * 2f + 0.1f;
			float d = 0.4f + num5 / 2f;
			float height = CapsuleHeight + 0.01f - num5 / 2f;
			chargeValidColliders.Clear();
			chargeValidColliders.Add(GetComponent<Collider>());
			chargeValidColliders.Add(combatCircle.Collider);
			chargeValidColliders.Add(enemy.GetComponent<Collider>());
			chargeValidColliders.Add(enemy.combatCircle.Collider);
			Vector3 vector = base.transform.position;
			while (!flag)
			{
				num2 = Vector3.Distance(vector, enemy.transform.position);
				Vector3 vector2 = enemy.transform.position - vector;
				vector2.Normalize();
				Vector3 vector3 = vector + Vector3.up * d;
				flag = PandoraUtils.RectCast(vector3, vector2, num2, height, num5, LayerMaskManager.chargeMask, chargeValidColliders, out raycastHitInfo);
				if (!flag)
				{
					float num6 = 0f;
					float num7 = 0f;
					Vector3 position3 = enemy.transform.position;
					float y2 = position3.y;
					Vector3 position4 = base.transform.position;
					if (y2 > position4.y)
					{
						Vector3 position5 = enemy.transform.position;
						num7 = position5.y;
						Vector3 position6 = base.transform.position;
						num6 = position6.y;
					}
					else
					{
						Vector3 position7 = base.transform.position;
						num7 = position7.y;
						Vector3 position8 = enemy.transform.position;
						num6 = position8.y;
					}
					if (num4 < 0.1f)
					{
						break;
					}
					Vector3 point = raycastHitInfo.point;
					float x = point.x;
					Vector3 point2 = raycastHitInfo.point;
					if (Vector2.SqrMagnitude(new Vector2(x, point2.z) - new Vector2(vector.x, vector.z)) < 0.1f)
					{
						break;
					}
					Vector3 point3 = raycastHitInfo.point;
					if (!PandoraUtils.IsBetween(point3.y, num6, num7) || raycastHitInfo.collider == null || raycastHitInfo.collider.gameObject.layer == LayerMaskManager.charactersLayer || raycastHitInfo.collider.gameObject.layer == LayerMaskManager.engage_circlesLayer)
					{
						break;
					}
					vector = vector3 + vector2 * raycastHitInfo.distance;
				}
			}
		}
		return flag;
	}

	public void SetFleeTarget()
	{
		FleeTarget = Vector3.zero;
		for (int i = 0; i < EngagedUnits.Count; i++)
		{
			FleeTarget += EngagedUnits[i].transform.position;
		}
		FleeTarget /= (float)EngagedUnits.Count;
	}

	public bool CanDisengage()
	{
		SetFleeTarget();
		float num = 0.2f;
		float num2 = CapsuleHeight - num + 0.01f;
		float width = CapsuleRadius * 2f + num + 0.01f;
		Vector3 position = base.transform.position;
		position.y = 0f;
		Vector3 fleeTarget = FleeTarget;
		fleeTarget.y = 0f;
		Vector3 normalized = (position - fleeTarget).normalized;
		return PandoraUtils.RectCast(base.transform.position + normalized * CapsuleRadius * 0.8f + Vector3.up * (num2 / 2f + num), normalized, 1f, num2, width, LayerMaskManager.chargeMask, null, out raycastHitInfo);
	}

	public BoneId GetMirrorBone(BoneId bone)
	{
		return PandoraSingleton<DataFactory>.Instance.InitData<BoneData>((int)bone).BoneIdMirror;
	}

	public void InitBoneTargets()
	{
		boneTargets = new List<BoneTarget>();
		if (unit.Status == UnitStateId.OUT_OF_ACTION)
		{
			return;
		}
		Transform transform = base.transform;
		List<BoneData> list = PandoraSingleton<DataFactory>.Instance.InitData<BoneData>();
		for (int i = 0; i < list.Count; i++)
		{
			BoneData boneData = list[i];
			if (boneData.IsRange && !unit.IsBoneBlocked(boneData.Id))
			{
				Transform transform2 = base.BonesTr[boneData.Id];
				Vector3 vector = transform.InverseTransformPoint(transform2.TransformPoint(Vector3.zero));
				vector.x = Mathf.Clamp(vector.x, 0f - CapsuleRadius + 0.1f, CapsuleRadius - 0.1f);
				vector.y = Mathf.Clamp(vector.y, 0.1f, CapsuleHeight - 0.1f);
				vector.z = Mathf.Clamp(vector.z, 0f - CapsuleRadius + 0.1f, CapsuleRadius - 0.1f);
				if (vector.y < CapsuleRadius)
				{
					Vector3 vector2 = new Vector3(0f, CapsuleRadius, 0f);
					vector = vector2 + (vector - vector2).normalized * (CapsuleRadius - 0.1f);
				}
				else if (vector.y > CapsuleHeight - CapsuleRadius)
				{
					Vector3 vector3 = new Vector3(0f, CapsuleHeight - CapsuleRadius, 0f);
					vector = vector3 + (vector - vector3).normalized * (CapsuleRadius - 0.1f);
				}
				boneTargets.Add(new BoneTarget
				{
					bone = boneData.Id,
					position = vector,
					transform = transform2
				});
			}
		}
	}

	public void OnDrawGizmos()
	{
		if (boneTargets != null && unit.Status != UnitStateId.OUT_OF_ACTION)
		{
			for (int i = 0; i < boneTargets.Count; i++)
			{
				Gizmos.DrawWireSphere(base.transform.TransformPoint(boneTargets[i].position), 0.1f);
			}
		}
	}

	public void InitTargetsData()
	{
		List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
		targetsData.Clear();
		for (int i = 0; i < allUnits.Count; i++)
		{
			if (allUnits[i] != this)
			{
				targetsData.Add(new TargetData(allUnits[i]));
			}
		}
	}

	public void UpdateTargetsData()
	{
		Vector3 raySrc = base.transform.position + Vector3.up * 1.4f;
		float maxSqrDist = unit.ViewDistance * unit.ViewDistance;
		for (int i = 0; i < targetsData.Count; i++)
		{
			if (targetsData[i].unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
			{
				UpdateTargetData(raySrc, maxSqrDist, targetsData[i]);
			}
		}
	}

	public void UpdateTargetData(UnitController target)
	{
		Vector3 raySrc = base.transform.position + Vector3.up * 1.4f;
		float maxSqrDist = unit.ViewDistance * unit.ViewDistance;
		int num = 0;
		while (true)
		{
			if (num < targetsData.Count)
			{
				if (targetsData[num].unitCtrlr == target)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		UpdateTargetData(raySrc, maxSqrDist, targetsData[num]);
	}

	private void UpdateTargetData(Vector3 raySrc, float maxSqrDist, TargetData targetData)
	{
		for (int i = 0; i < targetData.boneTargetRange.Count; i++)
		{
			Vector3 vector = targetData.unitCtrlr.transform.TransformPoint(targetData.unitCtrlr.boneTargets[i].position);
			float num = Vector3.SqrMagnitude(raySrc - vector);
			if (num <= maxSqrDist)
			{
				float dist = Mathf.Sqrt(num);
				BoneTargetRange boneTargetRange = targetData.boneTargetRange[i];
				SendTargetRay(raySrc, vector, dist, LayerMaskManager.rangeTargetMaskNoChar, targetData.unitCtrlr, boneTargetRange);
				targetData.boneTargetRangeBlockingUnit[i].hitBone = boneTargetRange.hitBone;
				targetData.boneTargetRangeBlockingUnit[i].hitPoint = boneTargetRange.hitPoint;
				targetData.boneTargetRangeBlockingUnit[i].distance = boneTargetRange.distance;
				if (boneTargetRange.hitBone)
				{
					SendTargetRay(raySrc, vector, dist, LayerMaskManager.rangeTargetMask, targetData.unitCtrlr, targetData.boneTargetRangeBlockingUnit[i]);
				}
			}
			else
			{
				targetData.boneTargetRange[i].hitBone = false;
				targetData.boneTargetRangeBlockingUnit[i].hitBone = false;
			}
		}
	}

	private void SendTargetRay(Vector3 raySrc, Vector3 rayDst, float dist, LayerMask mask, UnitController checkedUnit, BoneTargetRange boneTarget)
	{
		int num = Physics.RaycastNonAlloc(raySrc, rayDst - raySrc, PandoraUtils.hits, dist, mask);
		boneTarget.hitBone = (num == 0 || PandoraUtils.hits[0].transform.gameObject == checkedUnit.gameObject);
		boneTarget.hitPoint = ((num != 0) ? PandoraUtils.hits[0].point : rayDst);
		boneTarget.distance = dist;
	}

	public bool IsInRange(UnitController target, float minDistance, float maxDistance, float requiredPerc, bool unitBlocking, bool checkAllBones, BoneId requiredBoneId)
	{
		if (target == this)
		{
			return true;
		}
		for (int i = 0; i < targetsData.Count; i++)
		{
			if (targetsData[i].unitCtrlr == target)
			{
				return IsInRange(targetsData[i], minDistance, maxDistance, requiredPerc, unitBlocking, checkAllBones, requiredBoneId);
			}
		}
		return false;
	}

	private bool IsInRange(TargetData targetData, float minDistance, float maxDistance, float requiredPerc, bool unitBlocking, bool checkAllBones, BoneId requiredBoneId)
	{
		bool flag = false;
		bool flag2 = true;
		List<BoneTargetRange> list = (!unitBlocking) ? targetData.boneTargetRange : targetData.boneTargetRangeBlockingUnit;
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].hitBone)
			{
				continue;
			}
			if (list[i].distance >= minDistance && list[i].distance <= maxDistance)
			{
				if (targetData.unitCtrlr.boneTargets[i].bone == requiredBoneId)
				{
					flag = true;
				}
				else if (!checkAllBones)
				{
					return true;
				}
			}
			else
			{
				flag2 = false;
			}
		}
		if (requiredBoneId != 0 && !flag)
		{
			return false;
		}
		if (!flag2)
		{
			return false;
		}
		return 1f - targetData.GetCover(unitBlocking) >= requiredPerc;
	}

	public TargetData GetTargetData(UnitController ctrlr)
	{
		for (int i = 0; i < targetsData.Count; i++)
		{
			if (targetsData[i].unitCtrlr == ctrlr)
			{
				return targetsData[i];
			}
		}
		return null;
	}

	public bool CanTargetFromPoint(TargetData targetData, float minDistance, float maxDistance, float requiredPerc, bool unitBlocking, bool checkAllBones, BoneId requiredBoneId = BoneId.NONE)
	{
		return CanTargetFromPoint(base.transform.position + Vector3.up * 1.4f, targetData, minDistance, maxDistance, requiredPerc, unitBlocking, checkAllBones, requiredBoneId);
	}

	public bool CanTargetFromPoint(Vector3 pos, TargetData targetData, float minDistance, float maxDistance, float requiredPerc, bool unitBlocking, bool checkAllBones, BoneId requiredBoneId = BoneId.NONE)
	{
		UpdateTargetData(pos, maxDistance * maxDistance, targetData);
		return IsInRange(targetData, minDistance, maxDistance, requiredPerc, unitBlocking, checkAllBones, requiredBoneId);
	}

	public bool HasEnemyInSight()
	{
		return HasEnemyInSight(unit.ViewDistance);
	}

	public bool HasEnemyInSight(float dist)
	{
		float @float = Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC);
		List<UnitController> aliveEnemies = PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(GetWarband().idx);
		for (int i = 0; i < aliveEnemies.Count; i++)
		{
			if (IsInRange(aliveEnemies[i], 0f, dist, @float, unitBlocking: false, checkAllBones: false, BoneId.NONE))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasIdolInSight()
	{
		Vector3 vector = base.transform.position + Vector3.up * 1.4f;
		float num = unit.ViewDistance * unit.ViewDistance;
		int teamIdx = GetWarband().teamIdx;
		for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.MapImprints.Count; i++)
		{
			MapImprint mapImprint = PandoraSingleton<MissionManager>.Instance.MapImprints[i];
			if (mapImprint.Destructible != null && mapImprint.Destructible.Owner != null && mapImprint.Destructible.Owner.GetWarband().teamIdx != teamIdx)
			{
				Vector3 a = mapImprint.Destructible.transform.position + Vector3.up;
				if (Physics.Raycast(vector, a - vector, out raycastHitInfo, unit.ViewDistance, LayerMaskManager.groundMask) && raycastHitInfo.transform.parent != null && raycastHitInfo.transform.parent.gameObject == mapImprint.Destructible.gameObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void InitDefense(UnitController atkr, bool face = true)
	{
		attackerCtrlr = atkr;
		unit.PreviousStatus = unit.Status;
		if (face && unit.IsAvailable())
		{
			FaceTarget(attackerCtrlr.transform);
		}
	}

	public void EndDefense()
	{
		TriggerEnchantments(EnchantmentTriggerId.ON_END_DEFENSE);
		unit.DestroyEnchantments(EnchantmentTriggerId.ON_END_DEFENSE);
		if (AICtrlr != null && attackerCtrlr != null)
		{
			GetWarband().BlackListWarband(attackerCtrlr.GetWarband().idx);
		}
		if (unit.Status == UnitStateId.STUNNED)
		{
			StateMachine.ChangeState(9);
		}
		if (unit.Status == UnitStateId.OUT_OF_ACTION)
		{
			PandoraDebug.LogDebug("EndDefense, This Unit is dead = " + this);
			KillUnit();
		}
	}

	public void SetHitData(Transform point, Quaternion rot)
	{
		hitPoint = point;
	}

	public int GetFallHeight(UnitActionId unitActionId)
	{
		if (interactivePoint != null && unitActionId == UnitActionId.LEAP)
		{
			unitActionId = ((ActionZone)interactivePoint).GetJump().actionId;
		}
		int result = 0;
		switch (unitActionId)
		{
		case UnitActionId.CLIMB_3M:
		case UnitActionId.JUMP_3M:
			result = 3;
			break;
		case UnitActionId.CLIMB_6M:
		case UnitActionId.JUMP_6M:
			result = 6;
			break;
		case UnitActionId.CLIMB_9M:
		case UnitActionId.JUMP_9M:
			result = 9;
			break;
		}
		return result;
	}

	public void RemoveAthletics()
	{
		interactivePoint = null;
		activeActionDest = null;
		interactivePoints.Clear();
	}

	public void ResetAttackResult()
	{
		attackResultId = AttackResultId.NONE;
		lastActionWounds = 0;
		flyingLabel = string.Empty;
	}

	public int GetMeleeHitRoll(bool updateModifiers = false)
	{
		UnitController unitController = defenderCtrlr;
		if (unitController == null && EngagedUnits.Count > 0 && CurrentAction != null && CurrentAction.ActionId == UnitActionId.MELEE_ATTACK)
		{
			unitController = EngagedUnits[0];
		}
		else if (unitController == null && chargeTargets.Count > 0 && CurrentAction != null && CurrentAction.ActionId == UnitActionId.CHARGE)
		{
			unitController = chargeTargets[0];
		}
		else
		{
			if (unitController == null && destructibleTarget == null && CurrentAction != null && CurrentAction.Destructibles.Count > 0)
			{
				destructibleTarget = CurrentAction.Destructibles[0];
			}
			if (destructibleTarget != null)
			{
				return 100;
			}
		}
		if (unitController == null)
		{
			return 0;
		}
		if (updateModifiers)
		{
			CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.COMBAT_MELEE_HIT_ROLL));
		}
		int num = 0;
		num = unit.CombatMeleeHitRoll;
		if (unitController != null)
		{
			if (unitController.Fleeing)
			{
				num += Constant.GetInt(ConstantId.FLEE_MELEE_HIT_ROLL_MODIFIER);
			}
			else
			{
				num -= unitController.unit.MeleeResistance;
				if (updateModifiers)
				{
					CurrentRollModifiers.AddRange(unitController.unit.attributeModifiers.GetOrNull(AttributeId.MELEE_RESISTANCE), null, isPercent: false, isEnemyMod: true, negate: true);
				}
			}
		}
		return Mathf.Clamp(num, 1, Constant.GetInt(ConstantId.MAX_ROLL));
	}

	public int GetRangeHitRoll(bool updateModifiers = false)
	{
		return GetRangeHitRoll(base.transform, updateModifiers);
	}

	public int GetRangeHitRoll(Transform trans, bool updateModifiers = false)
	{
		if (Engaged)
		{
			return 0;
		}
		UnitController unitController = defenderCtrlr;
		if (unitController == null && CurrentAction != null && CurrentAction.Targets.Count > 0)
		{
			unitController = CurrentAction.Targets[0];
		}
		if (unitController == null && destructibleTarget == null && CurrentAction != null && CurrentAction.Destructibles.Count > 0)
		{
			destructibleTarget = CurrentAction.Destructibles[0];
		}
		if (destructibleTarget != null)
		{
			return 100;
		}
		if (unitController == null)
		{
			return 0;
		}
		if (updateModifiers)
		{
			CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.COMBAT_RANGE_HIT_ROLL));
		}
		int num = unit.CombatRangeHitRoll;
		if (unitController != null)
		{
			if (unitController.unit.RangeResistance > 0)
			{
				num -= Mathf.Max(unitController.unit.RangeResistance + unit.RangeResistanceDefenderModifier, 0);
				if (updateModifiers)
				{
					CurrentRollModifiers.AddRange(unitController.unit.attributeModifiers.GetOrNull(AttributeId.RANGE_RESISTANCE), null, isPercent: false, isEnemyMod: true, negate: true);
					CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.RANGE_RESISTANCE_DEFENDER_MODIFIER), null, isPercent: false, isEnemyMod: false, negate: true);
				}
			}
			Vector3 position = trans.position;
			float y = position.y;
			Vector3 position2 = unitController.transform.position;
			if (y >= position2.y + 2.8f)
			{
				num += unit.GetAttribute(AttributeId.RANGE_HIT_ROLL_BONUS_HIGHER);
				if (updateModifiers)
				{
					CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.RANGE_HIT_ROLL_BONUS_HIGHER));
				}
			}
			else
			{
				Vector3 position3 = trans.position;
				float y2 = position3.y;
				Vector3 position4 = unitController.transform.position;
				if (y2 <= position4.y - 2.8f)
				{
					num += unit.GetAttribute(AttributeId.RANGE_HIT_ROLL_BONUS_LOWER);
					if (updateModifiers)
					{
						CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.RANGE_HIT_ROLL_BONUS_LOWER));
					}
				}
			}
			if (unitController.unit.Status == UnitStateId.STUNNED)
			{
				num += unit.GetAttribute(AttributeId.RANGE_HIT_ROLL_BONUS_STUNNED);
				if (updateModifiers)
				{
					CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.RANGE_HIT_ROLL_BONUS_STUNNED));
				}
			}
			if (unitController.Engaged)
			{
				num += unit.GetAttribute(AttributeId.RANGE_HIT_ROLL_BONUS_ENGAGED);
				if (updateModifiers)
				{
					CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.RANGE_HIT_ROLL_BONUS_ENGAGED));
				}
			}
			TargetData targetData = GetTargetData(unitController);
			if (targetData != null)
			{
				float cover = targetData.GetCover(blockingUnit: true);
				if ((double)cover > 0.5)
				{
					num += Constant.GetInt(ConstantId.RANGE_BONUS_COVER_HIGH);
					if (updateModifiers)
					{
						CurrentRollModifiers.Add(new AttributeMod(AttributeMod.Type.NONE, PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("constant_", ConstantId.RANGE_BONUS_COVER_HIGH.ToString()), Constant.GetInt(ConstantId.RANGE_BONUS_COVER_HIGH)));
					}
				}
				else if ((double)cover > 0.25)
				{
					num += Constant.GetInt(ConstantId.RANGE_BONUS_COVER_LOW);
					if (updateModifiers)
					{
						CurrentRollModifiers.Add(new AttributeMod(AttributeMod.Type.NONE, PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("constant_", ConstantId.RANGE_BONUS_COVER_LOW.ToString()), Constant.GetInt(ConstantId.RANGE_BONUS_COVER_LOW)));
					}
				}
			}
		}
		return Mathf.Clamp(num, 1, Constant.GetInt(ConstantId.MAX_ROLL));
	}

	public int GetSpellCastingRoll(SpellTypeId typeId, bool updateModifiers = false)
	{
		int num = unit.SpellcastingRoll;
		if (updateModifiers)
		{
			CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.SPELLCASTING_ROLL));
		}
		switch (typeId)
		{
		case SpellTypeId.ARCANE:
			num += unit.ArcaneSpellcastingRoll;
			if (updateModifiers)
			{
				CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.ARCANE_SPELLCASTING_ROLL));
			}
			break;
		case SpellTypeId.DIVINE:
			num += unit.DivineSpellcastingRoll;
			if (updateModifiers)
			{
				CurrentRollModifiers.AddRange(unit.attributeModifiers.GetOrNull(AttributeId.DIVINE_SPELLCASTING_ROLL));
			}
			break;
		}
		return Mathf.Clamp(num, 1, Constant.GetInt(ConstantId.MAX_ROLL));
	}

	public void ComputeWound()
	{
		defenderCtrlr.wasMaxWound = (defenderCtrlr.unit.CurrentWound == defenderCtrlr.unit.Wound);
		defenderCtrlr.unit.PreviousStatus = defenderCtrlr.unit.Status;
		int num = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(CurrentAction.GetMinDamage(), CurrentAction.GetMaxDamage() + 1);
		int num2 = 0;
		AttributeId attributeId = AttributeId.NONE;
		if (HasClose())
		{
			num2 = unit.CriticalMeleeAttemptRoll;
			attributeId = AttributeId.CRITICAL_MELEE_ATTEMPT_ROLL;
		}
		else
		{
			num2 = unit.CriticalRangeAttemptRoll;
			attributeId = AttributeId.CRITICAL_RANGE_ATTEMPT_ROLL;
		}
		num2 += ((defenderCtrlr.unit.Status == UnitStateId.STUNNED) ? Constant.GetInt(ConstantId.CRIT_RANGE_BONUS_STUNNED) : 0);
		num2 -= defenderCtrlr.unit.CritResistance;
		criticalHit = unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, num2, attributeId);
		defenderCtrlr.criticalHit = criticalHit;
		if (criticalHit)
		{
			PandoraDebug.LogDebug("Critical Hit!");
			num = CurrentAction.GetMaxDamage(critical: true);
			List<CriticalEffectData> datas = PandoraSingleton<DataFactory>.Instance.InitData<CriticalEffectData>();
			CriticalEffectData randomRatio = CriticalEffectData.GetRandomRatio(datas, PandoraSingleton<MissionManager>.Instance.NetworkTyche);
			if (defenderCtrlr.unit.Status < randomRatio.UnitStateId)
			{
				defenderCtrlr.unit.SetStatus(randomRatio.UnitStateId);
				if (IsPlayed() && !defenderCtrlr.IsPlayed() && defenderCtrlr.unit.Status == UnitStateId.STUNNED)
				{
					PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.STUN_ENEMIES, 1);
				}
			}
			if (randomRatio.ApplyDebuf)
			{
				defenderCtrlr.unit.AddEnchantment(EnchantmentId.OPEN_WOUND, unit, original: false);
			}
			if (IsPlayed() && !defenderCtrlr.IsPlayed())
			{
				PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.CRITICALS, 1);
			}
		}
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_DAMAGE, defenderCtrlr, num, criticalHit);
		if (num == 0)
		{
			attackResultId = AttackResultId.HIT_NO_WOUND;
			defenderCtrlr.attackResultId = AttackResultId.HIT_NO_WOUND;
			return;
		}
		attackResultId = AttackResultId.HIT;
		defenderCtrlr.attackResultId = AttackResultId.HIT;
		IncrementDamageDoneStats(defenderCtrlr.unit, num, criticalHit);
		PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, (!criticalHit) ? CombatLogger.LogMessage.DAMAGE_INFLICT : CombatLogger.LogMessage.DAMAGE_CRIT_INFLICT, GetLogName(), num.ToConstantString(), defenderCtrlr.GetLogName());
		defenderCtrlr.unit.CurrentWound -= num;
		defenderCtrlr.lastActionWounds -= num;
		defenderCtrlr.flyingLabel = PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_value", defenderCtrlr.lastActionWounds.ToConstantString());
		TriggerEnchantments(EnchantmentTriggerId.ON_DAMAGE);
		defenderCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_DAMAGE_RECEIVED);
		if (CurrentAction.ActionId == UnitActionId.MELEE_ATTACK || CurrentAction.ActionId == UnitActionId.CHARGE || CurrentAction.ActionId == UnitActionId.AMBUSH)
		{
			TriggerEnchantments(EnchantmentTriggerId.ON_MELEE_DAMAGE);
			TriggerEnchantments(EnchantmentTriggerId.ON_MELEE_DAMAGE_RANDOM);
			defenderCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_MELEE_DAMAGE_RECEIVED);
			if (CurrentAction.ActionId == UnitActionId.CHARGE || CurrentAction.ActionId == UnitActionId.AMBUSH)
			{
				TriggerEnchantments(EnchantmentTriggerId.ON_CHARGE_DAMAGE);
				defenderCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_CHARGE_DAMAGE_RECEIVED);
			}
		}
		if (CurrentAction.ActionId == UnitActionId.SHOOT || CurrentAction.ActionId == UnitActionId.AIM || CurrentAction.ActionId == UnitActionId.OVERWATCH)
		{
			TriggerEnchantments(EnchantmentTriggerId.ON_RANGE_DAMAGE);
			defenderCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_RANGE_DAMAGE_RECEIVED);
		}
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ATTRIBUTES_CHANGED, defenderCtrlr.unit);
		defenderCtrlr.CheckOutOfAction(this);
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(defenderCtrlr);
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(this);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ATTRIBUTES_CHANGED, defenderCtrlr.unit);
	}

	public void ComputeDestructibleWound(Destructible dest)
	{
		int num = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(CurrentAction.GetMinDamage(), CurrentAction.GetMaxDamage() + 1);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_DAMAGE, dest, num, v3: false);
		attackResultId = AttackResultId.HIT;
		IncrementDamageDoneStats(num);
		PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, CombatLogger.LogMessage.DAMAGE_INFLICT, GetLogName(), num.ToConstantString(), PandoraSingleton<LocalizationManager>.Instance.GetStringById(dest.Data.Name));
		dest.ApplyDamage(num);
	}

	public void ComputeDirectWound(int damage, bool byPassArmor, UnitController damageDealer, bool fake = false)
	{
		wasMaxWound = (unit.CurrentWound == unit.Wound);
		criticalHit = false;
		if (defenderCtrlr != null)
		{
			defenderCtrlr.criticalHit = false;
		}
		unit.PreviousStatus = unit.Status;
		flyingLabel = string.Empty;
		if (!byPassArmor)
		{
			damage = Mathf.Max(0, damage - unit.ArmorAbsorption);
		}
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_DAMAGE, this, damage, criticalHit);
		if (damage == 0)
		{
			attackResultId = AttackResultId.HIT_NO_WOUND;
			return;
		}
		if (attackerCtrlr != null)
		{
			attackerCtrlr.IncrementDamageDoneStats(unit, damage, critical: false);
			PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, (damage <= 0) ? CombatLogger.LogMessage.HEALING_RECEIVED : CombatLogger.LogMessage.DAMAGE_INFLICT, attackerCtrlr.GetLogName(), Mathf.Abs(damage).ToConstantString(), GetLogName());
		}
		else
		{
			PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, (damage <= 0) ? CombatLogger.LogMessage.HEALING : CombatLogger.LogMessage.DAMAGE, GetLogName(), Mathf.Abs(damage).ToConstantString());
		}
		if (!fake)
		{
			unit.CurrentWound -= damage;
		}
		lastActionWounds -= damage;
		attackResultId = AttackResultId.HIT;
		flyingLabel = PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_value", lastActionWounds.ToConstantString());
		CheckOutOfAction(damageDealer);
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(damageDealer);
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(this);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ATTRIBUTES_CHANGED, unit);
	}

	public void CheckOutOfAction(UnitController damageDealer)
	{
		if (unit.CurrentWound > 0)
		{
			return;
		}
		PandoraDebug.LogDebug("Unit is Dead! current Wound < 0", "DEATH", this);
		if (damageDealer != null && damageDealer.IsPlayed() && !IsPlayed())
		{
			if (unit.Status == UnitStateId.STUNNED)
			{
				PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.STUNNED_OOAS, 1);
			}
			if (unit.GetUnitTypeId() == UnitTypeId.IMPRESSIVE)
			{
				PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.IMPRESSIVE_OOAS, 1);
			}
			PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.ENEMIES_OOA, 1);
		}
		if (IsPlayed())
		{
			PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.MY_TOTAL_OOA, 1);
			if (wasMaxWound)
			{
				PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.ONE_SHOT);
			}
		}
		unit.SetStatus(UnitStateId.OUT_OF_ACTION);
		unit.AddEnchantment(EnchantmentId.INJURY_ROLL, unit, original: false);
	}

	public void ResurectUnit()
	{
		Resurected = true;
		GetComponent<Collider>().enabled = true;
		Imprint.alive = true;
		Imprint.alwaysHide = false;
		Imprint.alwaysVisible = false;
		killed = false;
		unit.Resurect();
		animator.Play(AnimatorIds.idle);
		StartGameInitialization();
		PandoraSingleton<MissionManager>.Instance.ForceUnitVisibilityCheck(this);
		combatCircle.gameObject.SetActive(value: true);
		SetCombatCircle(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit(), forced: true);
		StateMachine.ChangeState(9);
		PandoraSingleton<MissionManager>.Instance.resendLadder = true;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_UNIT_SPAWN, "mission_unit_resurection", unit.Name);
	}

	public void KillUnit()
	{
		if (killed)
		{
			return;
		}
		killed = true;
		unit.CleanEnchantments();
		GetComponent<Collider>().enabled = false;
		GetWarband().MoralValue -= unit.MoralImpact;
		if (attackerCtrlr != null)
		{
			PandoraDebug.LogDebug("Increment Kill Stat!", "DEATH", this);
			attackerCtrlr.IncrementKillStat(unit);
		}
		Imprint.alive = false;
		List<Item> list = new List<Item>();
		if (unit.RaceId != RaceId.DAEMON && !unit.NoLootBag)
		{
			if (unit.deathTrophy.Id != 0)
			{
				list.Add(unit.deathTrophy);
				unit.deathTrophy.Save.ownerMyrtilus = uid;
			}
			for (int num = unit.Items.Count - 1; num >= 0; num--)
			{
				UnitSlotId unitSlotId = (UnitSlotId)num;
				if (unitSlotId != UnitSlotId.ARMOR && unitSlotId != 0 && ((unitSlotId == UnitSlotId.SET1_OFFHAND && !unit.Items[num - 1].IsPaired) || unitSlotId != UnitSlotId.SET1_OFFHAND) && ((unitSlotId == UnitSlotId.SET2_MAINHAND && !unit.Items[num - 2].IsLockSlot) || unitSlotId != UnitSlotId.SET2_MAINHAND) && ((unitSlotId == UnitSlotId.SET2_MAINHAND && unit.GetMutationId(unitSlotId) == MutationId.NONE) || unitSlotId != UnitSlotId.SET2_MAINHAND) && ((unitSlotId == UnitSlotId.SET2_OFFHAND && unit.GetMutationId(unitSlotId) == MutationId.NONE) || unitSlotId != UnitSlotId.SET2_OFFHAND) && ((unitSlotId == UnitSlotId.SET2_OFFHAND && !unit.Items[num - 2].IsLockSlot) || unitSlotId != UnitSlotId.SET2_OFFHAND) && ((unitSlotId == UnitSlotId.SET2_OFFHAND && !unit.Items[num - 1].IsPaired) || unitSlotId != UnitSlotId.SET2_OFFHAND))
				{
					List<Item> list2 = unit.EquipItem((UnitSlotId)num, new Item(ItemId.NONE), sortItems: false);
					for (int i = 0; i < list2.Count; i++)
					{
						Item item = list2[i];
						for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; j++)
						{
							if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].ItemIdol == item)
							{
								PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].RemoveMoralIdol();
							}
						}
						if (item.Id != 0)
						{
							if (!item.IsTrophy)
							{
								item.owner = unit;
							}
							item.Save.oldSlot = (int)unitSlotId;
							list.Add(item);
						}
					}
				}
			}
		}
		if (list.Count > 0)
		{
			lootBagPoint = PandoraSingleton<MissionManager>.Instance.SpawnLootBag(this, base.transform.position, list, visible: false, wasSearched: false);
		}
		List<UnitController> list3 = new List<UnitController>();
		list3.AddRange(EngagedUnits);
		SetGraphWalkability(walkable: true);
		StateMachine.ChangeState(9);
		PandoraSingleton<MissionManager>.Instance.resendLadder = true;
		if (unit.Data.ZoneAoeIdDeathSpawn != 0)
		{
			ZoneAoe.Spawn(unit.Data.ZoneAoeIdDeathSpawn, 4f, base.transform.position, this);
		}
		if (unit.Data.UnitIdDeathSpawn != 0)
		{
			WarbandController warband = GetWarband();
			int num2 = 0;
			Vector2 a = default(Vector2);
			while (num2 < unit.Data.DeathSpawnCount)
			{
				UnitController unitController = PandoraSingleton<MissionManager>.Instance.excludedUnits.Find((UnitController x) => x.unit.Id == unit.Data.UnitIdDeathSpawn);
				if (unitController != null)
				{
					Vector3 pos = base.transform.position;
					Quaternion rotation = base.transform.rotation;
					bool flag = true;
					if (num2 > 0)
					{
						float num3 = Constant.GetFloat(ConstantId.MELEE_RANGE_LARGE) + 0.1f;
						float num4 = unitController.CapsuleRadius + 0.1f;
						float num5 = MathF.PI * 2f * num3;
						float num6 = num5 / num4;
						float num7 = 360f / num6;
						float num8 = 0f;
						bool flag2 = false;
						for (; num8 < 360f; num8 += num7)
						{
							if (flag2)
							{
								break;
							}
							Vector3 forward = Vector3.forward;
							forward = Quaternion.Euler(0f, num8, 0f) * forward;
							forward.Normalize();
							Vector3 vector = base.transform.position + forward * num3;
							Vector2 vector2 = new Vector2(vector.x, vector.z);
							if (PandoraUtils.SendCapsule(base.transform.position, forward, num4 + 0.1f, 1.5f, num3, num4))
							{
								continue;
							}
							bool flag3 = true;
							UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
							List<UnitController> allAliveUnits = PandoraSingleton<MissionManager>.Instance.GetAllAliveUnits();
							for (int k = 0; k < allAliveUnits.Count; k++)
							{
								Vector3 position = allAliveUnits[k].transform.position;
								float x2 = position.x;
								Vector3 position2 = allAliveUnits[k].transform.position;
								a = new Vector2(x2, position2.z);
								float d = 100f;
								Vector2 checkDestPoint = vector2 + (a - vector2) * d;
								if ((allAliveUnits[k] != currentUnit && PandoraUtils.IsPointInsideEdges(allAliveUnits[k].combatCircle.Edges, vector2, checkDestPoint)) || (allAliveUnits[k] == currentUnit && Vector3.SqrMagnitude(vector - currentUnit.transform.position) < num3 * num3))
								{
									flag3 = false;
									break;
								}
							}
							if (flag3)
							{
								pos = vector;
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							List<DecisionPoint> decisionPoints = PandoraSingleton<MissionManager>.Instance.GetDecisionPoints(this, DecisionPointId.SPAWN, 9999f);
							float num9 = float.MaxValue;
							DecisionPoint decisionPoint = null;
							for (int l = 0; l < decisionPoints.Count; l++)
							{
								float num10 = Vector3.SqrMagnitude(base.transform.position - decisionPoints[l].transform.position);
								if (num10 < num9)
								{
									num9 = num10;
									decisionPoint = decisionPoints[l];
								}
							}
							flag = (decisionPoint != null);
							if (flag)
							{
								pos = decisionPoint.transform.position;
							}
						}
					}
					if (flag)
					{
						PandoraSingleton<MissionManager>.Instance.IncludeUnit(unitController, pos, rotation);
						PandoraSingleton<MissionManager>.Instance.ForceUnitVisibilityCheck(unitController);
					}
					num2++;
				}
				else
				{
					num2 = unit.Data.DeathSpawnCount;
				}
			}
		}
		combatCircle.gameObject.SetActive(value: false);
		if (linkedSearchPoints != null && unlockSearchPointOnDeath)
		{
			unlockSearchPointOnDeath = false;
			for (int m = 0; m < linkedSearchPoints.Count; m++)
			{
				linkedSearchPoints[m].gameObject.SetActive(value: true);
			}
		}
		if (unit.CampaignData != null && unit.CampaignData.CampaignUnitIdSpawnOnDeath != 0 && !spawnedOnDeath)
		{
			spawnedOnDeath = true;
			PandoraSingleton<MissionManager>.Instance.ActivateHiddenUnit(unit.CampaignData.CampaignUnitIdSpawnOnDeath, spawnVisible: true, "mission_unit_resurection");
		}
		if (reviveUntilSearchEmpty)
		{
			bool flag4 = true;
			for (int n = 0; n < linkedSearchPoints.Count; n++)
			{
				if (!flag4)
				{
					break;
				}
				flag4 &= linkedSearchPoints[n].IsEmpty();
			}
			if (!flag4)
			{
				List<DecisionPoint> availableSpawnPoints = PandoraSingleton<MissionManager>.Instance.GetAvailableSpawnPoints(visible: false, asc: true, null, forcedSpawnPoints);
				int index = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, availableSpawnPoints.Count);
				base.transform.position = availableSpawnPoints[index].transform.position;
				base.transform.rotation = availableSpawnPoints[index].transform.rotation;
				ResurectUnit();
				list3.Add(this);
			}
		}
		EngagedUnits.Clear();
		for (int num11 = 0; num11 < list3.Count; num11++)
		{
			PandoraDebug.LogDebug("Check Engage for " + list3[num11].gameObject.name, "DEATH", this);
			list3[num11].CheckEngaged(applyEnchants: true);
		}
		PandoraSingleton<MissionManager>.Instance.CheckEndGame();
	}

	public void IncrementDamageDoneStats(Unit victim, int damage, bool critical)
	{
		damage = Mathf.Min(damage, victim.CurrentWound);
		unit.AddToAttribute(AttributeId.TOTAL_DAMAGE, damage);
		if (unit.warbandIdx == victim.warbandIdx)
		{
			AddMvuPoint(ConstantId.MVU_DMG_FRIENDLY, MvuCategory.FRIENDLY_DAMAGE, damage);
		}
		else
		{
			AddMvuPoint(ConstantId.MVU_DMG_ENEMY, MvuCategory.ENEMY_DAMAGE, damage);
		}
	}

	public void IncrementDamageDoneStats(int damage)
	{
		AddMvuPoint(ConstantId.MVU_DMG_ENEMY, MvuCategory.ENEMY_DAMAGE, damage);
	}

	public void IncrementKillStat(Unit victim)
	{
		if (unit.GetUnitTypeId() == UnitTypeId.MONSTER || unit.GetUnitTypeId() == UnitTypeId.DRAMATIS)
		{
			return;
		}
		switch (victim.GetUnitTypeId())
		{
		case UnitTypeId.IMPRESSIVE:
		case UnitTypeId.DRAMATIS:
			if (unit.warbandIdx == victim.warbandIdx)
			{
				AddMvuPoint(ConstantId.MVU_KILL_FRIENDLY_IMPRESSIVE, MvuCategory.FRIENDLY_OOA);
			}
			if (victim.CampaignData == null && victim.Id == UnitId.CHAOS_OGRE)
			{
				unit.AddToAttribute(AttributeId.TOTAL_KILL_ROAMING, 1);
			}
			else
			{
				AddMvuPoint(ConstantId.MVU_KILL_ENEMY_IMPRESSIVE, MvuCategory.ENEMY_OOA);
			}
			break;
		case UnitTypeId.LEADER:
			if (unit.warbandIdx == victim.warbandIdx)
			{
				AddMvuPoint(ConstantId.MVU_KILL_FRIENDLY_LEADER, MvuCategory.FRIENDLY_OOA);
			}
			else
			{
				AddMvuPoint(ConstantId.MVU_KILL_ENEMY_LEADER, MvuCategory.ENEMY_OOA);
			}
			break;
		case UnitTypeId.MONSTER:
			AddMvuPoint(ConstantId.MVU_KILL_MONSTER, MvuCategory.ENEMY_OOA);
			if (victim.CampaignData == null && victim.Id != UnitId.BLUE_HORROR)
			{
				unit.AddToAttribute(AttributeId.TOTAL_KILL_ROAMING, 1);
			}
			break;
		case UnitTypeId.HERO_1:
		case UnitTypeId.HERO_2:
		case UnitTypeId.HERO_3:
			if (unit.warbandIdx == victim.warbandIdx)
			{
				AddMvuPoint(ConstantId.MVU_KILL_FRIENDLY_HERO, MvuCategory.FRIENDLY_OOA);
			}
			else
			{
				AddMvuPoint(ConstantId.MVU_KILL_ENEMY_HERO, MvuCategory.ENEMY_OOA);
			}
			break;
		case UnitTypeId.HENCHMEN:
			if (unit.warbandIdx == victim.warbandIdx)
			{
				AddMvuPoint(ConstantId.MVU_KILL_FRIENDLY_HENCHMEN, MvuCategory.FRIENDLY_OOA);
			}
			else
			{
				AddMvuPoint(ConstantId.MVU_KILL_ENEMY_HENCHMEN, MvuCategory.ENEMY_OOA);
			}
			break;
		}
		unit.AddToAttribute(AttributeId.TOTAL_KILL, 1);
	}

	public void AddMvuPoint(ConstantId mvuConstant, MvuCategory category, int nb = 1)
	{
		int num = Constant.GetInt(mvuConstant) * nb;
		unit.AddToAttribute(AttributeId.CURRENT_MVU, num);
		unit.AddToAttribute(AttributeId.TOTAL_MVU, num);
		MVUptsPerCategory[(int)category] += num;
	}

	public bool IsTargeting()
	{
		return IsCurrentState(State.SINGLE_TARGETING) || IsCurrentState(State.AOE_TARGETING) || IsCurrentState(State.CONE_TARGETING) || IsCurrentState(State.LINE_TARGETING) || IsCurrentState(State.COUNTER_CHOICE) || IsCurrentState(State.INTERACTIVE_TARGET);
	}

	public bool IsChoosingTarget()
	{
		return IsCurrentState(State.SINGLE_TARGETING) || IsCurrentState(State.INTERACTIVE_TARGET);
	}

	public bool CanCounterAttack()
	{
		ActionStatus action = GetAction(SkillId.BASE_COUNTER_ATTACK);
		action.UpdateAvailable();
		return unit.IsAvailable() && !Fleeing && action.Available && unit.CounterDisabled == 0;
	}

	public bool IsBounty()
	{
		WarbandController warband = GetWarband();
		if (warband != null)
		{
			int idx = warband.idx;
			for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
			{
				WarbandController warbandController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i];
				if (warbandController.idx == idx)
				{
					continue;
				}
				for (int j = 0; j < warbandController.objectives.Count; j++)
				{
					if (warbandController.objectives[j].TypeId == PrimaryObjectiveTypeId.BOUNTY && ((ObjectiveBounty)warbandController.objectives[j]).IsUnitBounty(this))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void RegisterItems()
	{
		oldItems.Clear();
		for (int i = 6; i < unit.Items.Count; i++)
		{
			oldItems.Add(unit.Items[i]);
		}
	}

	public void GetAlliesEnemies(out List<UnitController> allies, out List<UnitController> enemies)
	{
		SeparateAlliesEnemies(PandoraSingleton<MissionManager>.Instance.GetAllUnits(), out allies, out enemies);
	}

	public void GetDefendersAlliesEnemies(out List<UnitController> allies, out List<UnitController> enemies)
	{
		SeparateAlliesEnemies(defenders, out allies, out enemies);
	}

	private void SeparateAlliesEnemies(List<UnitController> refList, out List<UnitController> allies, out List<UnitController> enemies)
	{
		allies = new List<UnitController>();
		enemies = new List<UnitController>();
		int teamIdx = GetWarband().teamIdx;
		for (int i = 0; i < refList.Count; i++)
		{
			if (refList[i] != this)
			{
				if (refList[i].GetWarband().teamIdx == teamIdx)
				{
					allies.Add(refList[i]);
				}
				else
				{
					enemies.Add(refList[i]);
				}
			}
		}
	}

	public void TriggerEnchantments(EnchantmentTriggerId triggerId, SkillId skillId = SkillId.NONE, UnitActionId actionId = UnitActionId.NONE)
	{
		List<UnitController> updatedUnits = new List<UnitController>();
		GetDefendersAlliesEnemies(out List<UnitController> allies, out List<UnitController> enemies);
		string text = ((int)triggerId).ToConstantString();
		List<SkillEnchantmentData> list = new List<SkillEnchantmentData>();
		if (CurrentAction != null)
		{
			list.AddRange(PandoraSingleton<DataFactory>.Instance.InitData<SkillEnchantmentData>(new string[2]
			{
				"fk_skill_id",
				"fk_enchantment_trigger_id"
			}, new string[2]
			{
				((int)CurrentAction.SkillId).ToConstantString(),
				text
			}));
			if ((triggerId == EnchantmentTriggerId.ON_SPELL_IMPACT_RANDOM || triggerId == EnchantmentTriggerId.ON_SKILL_IMPACT_RANDOM || triggerId == EnchantmentTriggerId.ON_MELEE_DAMAGE_RANDOM) && list.Count > 1)
			{
				int index = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list.Count);
				SkillEnchantmentData item = list[index];
				list.Clear();
				list.Add(item);
			}
		}
		for (int i = 0; i < unit.PassiveSkills.Count; i++)
		{
			List<SkillEnchantmentData> list2 = new List<SkillEnchantmentData>(PandoraSingleton<DataFactory>.Instance.InitData<SkillEnchantmentData>(new string[2]
			{
				"fk_skill_id",
				"fk_enchantment_trigger_id"
			}, new string[2]
			{
				((int)unit.PassiveSkills[i].Id).ToConstantString(),
				text
			}));
			if ((triggerId == EnchantmentTriggerId.ON_SPELL_IMPACT_RANDOM || triggerId == EnchantmentTriggerId.ON_SKILL_IMPACT_RANDOM || triggerId == EnchantmentTriggerId.ON_MELEE_DAMAGE_RANDOM) && list2.Count > 1)
			{
				int index2 = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list2.Count);
				SkillEnchantmentData item2 = list2[index2];
				list2.Clear();
				list2.Add(item2);
			}
			list.AddRange(list2);
		}
		for (int j = 0; j < list.Count; j++)
		{
			SkillEnchantmentData skillEnchantmentData = list[j];
			SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)skillEnchantmentData.SkillId);
			bool flag = true;
			if (skillEnchantmentData.Ratio != 0)
			{
				flag = (PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, 100) < skillEnchantmentData.Ratio);
			}
			if (!flag || skillEnchantmentData.EnchantmentTriggerId != triggerId || (skillEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.NONE && skillData.Passive) || ((skillEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.ON_ACTION || skillEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.ON_POST_ACTION || skillEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.ON_ATHLETIC_SUCCESS_ENGAGED) && (skillEnchantmentData.UnitActionIdTrigger != 0 || skillEnchantmentData.SkillIdTrigger != 0) && skillEnchantmentData.UnitActionIdTrigger != actionId && skillEnchantmentData.SkillIdTrigger != skillId))
			{
				continue;
			}
			if (skillEnchantmentData.Self)
			{
				AddTriggeredEnchantment(this, unit, skillEnchantmentData.EnchantmentId, ref updatedUnits);
			}
			if (skillEnchantmentData.TargetSelf && defenders.IndexOf(this) != -1)
			{
				AddTriggeredEnchantment(this, unit, skillEnchantmentData.EnchantmentId, ref updatedUnits);
			}
			if (skillEnchantmentData.TargetAlly && allies != null)
			{
				for (int k = 0; k < allies.Count; k++)
				{
					AddTriggeredEnchantment(allies[k], unit, skillEnchantmentData.EnchantmentId, ref updatedUnits);
				}
			}
			if (skillEnchantmentData.TargetEnemy && enemies != null)
			{
				for (int l = 0; l < enemies.Count; l++)
				{
					AddTriggeredEnchantment(enemies[l], unit, skillEnchantmentData.EnchantmentId, ref updatedUnits);
				}
			}
		}
		AddEnchantEffects(unit.Enchantments, defenders, allies, enemies, triggerId, actionId, skillId, ref updatedUnits);
		for (int m = 0; m < 6; m++)
		{
			if (m != (int)unit.InactiveWeaponSlot && m != (int)(unit.InactiveWeaponSlot + 1))
			{
				AddEnchantEffects(unit.Items[m].Enchantments, defenders, allies, enemies, triggerId, actionId, skillId, ref updatedUnits);
				if (unit.Items[m].RuneMark != null)
				{
					AddEnchantEffects(unit.Items[m].RuneMark.Enchantments, defenders, allies, enemies, triggerId, actionId, skillId, ref updatedUnits);
				}
			}
		}
		for (int n = 0; n < unit.Injuries.Count; n++)
		{
			AddEnchantEffects(unit.Injuries[n].Enchantments, defenders, allies, enemies, triggerId, actionId, skillId, ref updatedUnits);
		}
		for (int num = 0; num < unit.Mutations.Count; num++)
		{
			AddEnchantEffects(unit.Mutations[num].Enchantments, defenders, allies, enemies, triggerId, actionId, skillId, ref updatedUnits);
		}
		for (int num2 = 0; num2 < updatedUnits.Count; num2++)
		{
			updatedUnits[num2].unit.UpdateAttributes();
		}
	}

	private void AddEnchantEffects(List<Enchantment> enchants, List<UnitController> defenders, List<UnitController> allies, List<UnitController> enemies, EnchantmentTriggerId triggerId, UnitActionId actionId, SkillId skillId, ref List<UnitController> updatedUnits)
	{
		for (int num = enchants.Count - 1; num >= 0; num--)
		{
			for (int i = 0; i < enchants[num].Effects.Count; i++)
			{
				EnchantmentEffectEnchantmentData enchantmentEffectEnchantmentData = enchants[num].Effects[i];
				if (enchantmentEffectEnchantmentData.EnchantmentTriggerId != triggerId || ((enchantmentEffectEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.ON_ACTION || enchantmentEffectEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.ON_POST_ACTION || enchantmentEffectEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.ON_ATHLETIC_SUCCESS_ENGAGED) && (enchantmentEffectEnchantmentData.UnitActionIdTrigger != 0 || enchantmentEffectEnchantmentData.SkillIdTrigger != 0) && enchantmentEffectEnchantmentData.UnitActionIdTrigger != actionId && enchantmentEffectEnchantmentData.SkillIdTrigger != skillId))
				{
					continue;
				}
				bool flag = true;
				if (enchantmentEffectEnchantmentData.Ratio != 0)
				{
					flag = (PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, 100) < enchantmentEffectEnchantmentData.Ratio);
				}
				if (!flag)
				{
					continue;
				}
				AttributeId attributeModifierId = unit.GetAttributeModifierId(enchantmentEffectEnchantmentData.AttributeIdRoll);
				if (enchantmentEffectEnchantmentData.Self)
				{
					AddTriggeredEnchantment(this, unit, enchantmentEffectEnchantmentData.EnchantmentIdEffect, enchantmentEffectEnchantmentData.AttributeIdRoll, attributeModifierId, enchants[num].AllegianceId, ref updatedUnits);
				}
				if (enchantmentEffectEnchantmentData.TargetSelf && defenders.IndexOf(this) != -1)
				{
					AddTriggeredEnchantment(this, unit, enchantmentEffectEnchantmentData.EnchantmentIdEffect, enchantmentEffectEnchantmentData.AttributeIdRoll, attributeModifierId, enchants[num].AllegianceId, ref updatedUnits);
				}
				if (enchantmentEffectEnchantmentData.TargetAlly && allies != null)
				{
					for (int j = 0; j < allies.Count; j++)
					{
						AddTriggeredEnchantment(allies[j], unit, enchantmentEffectEnchantmentData.EnchantmentIdEffect, enchantmentEffectEnchantmentData.AttributeIdRoll, attributeModifierId, enchants[num].AllegianceId, ref updatedUnits);
					}
				}
				if (enchantmentEffectEnchantmentData.TargetEnemy && enemies != null)
				{
					for (int k = 0; k < enemies.Count; k++)
					{
						AddTriggeredEnchantment(enemies[k], unit, enchantmentEffectEnchantmentData.EnchantmentIdEffect, enchantmentEffectEnchantmentData.AttributeIdRoll, attributeModifierId, enchants[num].AllegianceId, ref updatedUnits);
					}
				}
			}
		}
	}

	public void AddTriggeredEnchantment(UnitController target, Unit provider, EnchantmentId effectId, ref List<UnitController> updatedUnits)
	{
		AddTriggeredEnchantment(target, provider, effectId, AttributeId.NONE, AttributeId.NONE, AllegianceId.NONE, ref updatedUnits);
	}

	public void AddTriggeredEnchantment(UnitController target, Unit provider, EnchantmentId effectId, AttributeId rollId, AttributeId rollModifierId, AllegianceId allegianceId, ref List<UnitController> updatedUnits)
	{
		bool flag = false;
		string v = string.Empty;
		if (rollId != 0)
		{
			int attribute = target.unit.GetAttribute(rollId);
			attribute += provider.GetAttributeModifier(rollId);
			flag = target.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, attribute, rollId, reverse: false, apply: false);
			if (flag)
			{
				v = PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_resist");
			}
		}
		Enchantment enchantment = null;
		if (!flag)
		{
			enchantment = target.unit.AddEnchantment(effectId, unit, original: false, updateAttributes: false, allegianceId);
			if (enchantment == null)
			{
				v = PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_immune");
			}
			if (updatedUnits.IndexOf(target) == -1)
			{
				updatedUnits.Add(target);
			}
		}
		if (enchantment != null && !enchantment.Data.NoDisplay)
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_ENCHANTMENT, target, enchantment.LocalizedName, enchantment.Data.EffectTypeId, v);
		}
	}

	public void Ground()
	{
		if (Physics.Linecast(base.transform.position + Vector3.up, base.transform.position + Vector3.up * -3f, out raycastHitInfo, LayerMaskManager.groundMask))
		{
			SetFixed(raycastHitInfo.point, fix: true);
		}
	}

	public void SpawnBeacon()
	{
		UpdateActionStatus(notice: true, UnitActionRefreshId.ON_ACTION);
		SetCurrentBeacon(PandoraSingleton<MissionManager>.Instance.SpawnBeacon(base.transform.position));
		PandoraSingleton<MissionManager>.Instance.MoveCircle.Show(startPosition, unit.Movement);
	}

	public void RevertBeacons(Beacon keavin)
	{
		if (!(keavin == CurrentBeacon))
		{
			SetCurrentBeacon(keavin);
			PandoraSingleton<MissionManager>.Instance.RevertBeacons(keavin);
			PandoraSingleton<MissionManager>.Instance.MoveCircle.Show(startPosition, unit.Movement);
		}
	}

	private void SetCurrentBeacon(Beacon keavin)
	{
		CurrentBeacon = keavin;
		startPosition = keavin.transform.position;
	}

	public void ValidMove()
	{
		unit.RemoveTempPoints();
		PandoraSingleton<MissionManager>.Instance.ClearBeacons();
		startPosition = base.transform.position;
		startRotation = base.transform.rotation;
	}

	public override void SetAnimSpeed(float speed)
	{
		if (Engaged && speed == 0f)
		{
			if (currentAnimSpeed > 0f)
			{
				currentAnimSpeed = 0f;
			}
		}
		else
		{
			currentAnimSpeed = speed;
			base.SetAnimSpeed(speed);
		}
	}

	public void SetCombatCircle(UnitController currentUnit, bool forced = false)
	{
		if (!forced && (currentUnit == this || unit.Status == UnitStateId.OUT_OF_ACTION))
		{
			combatCircle.Edges.Clear();
			combatCircle.gameObject.SetActive(value: false);
		}
		else
		{
			combatCircle.gameObject.SetActive(value: true);
			float num = 0f;
			Quaternion quaternion = currentUnit.transform.rotation;
			float sizeB;
			if (unit.Id == UnitId.MANTICORE || currentUnit.unit.Id == UnitId.MANTICORE)
			{
				num = Constant.GetFloat(ConstantId.MELEE_RANGE_VERY_LARGE);
				sizeB = Constant.GetFloat(ConstantId.MELEE_RANGE_VERY_VERY_LARGE);
				quaternion = ((unit.Id != UnitId.MANTICORE) ? quaternion : base.transform.rotation);
			}
			else
			{
				num = Constant.GetFloat((unit.Data.UnitSizeId != UnitSizeId.LARGE && currentUnit.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? ConstantId.MELEE_RANGE_NORMAL : ConstantId.MELEE_RANGE_LARGE);
				sizeB = num;
			}
			float currentUnitRadius = (currentUnit.unit.Id != UnitId.MANTICORE) ? currentUnit.CapsuleRadius : (currentUnit.CapsuleHeight / 2f);
			combatCircle.Set(IsEnemy(currentUnit), Engaged, currentUnit.IsPlayed(), num, sizeB, currentUnitRadius, quaternion);
		}
		SetGraphWalkability(!combatCircle.gameObject.activeSelf);
		SetCombatCircleAlpha(currentUnit);
	}

	public void SetCombatCircleAlpha(UnitController currentUnit)
	{
		if (currentUnit != this && unit.Status != UnitStateId.OUT_OF_ACTION && Imprint.State == MapImprintStateId.VISIBLE)
		{
			float num = Vector3.SqrMagnitude(currentUnit.transform.position - base.transform.position);
			combatCircle.SetAlpha(Mathf.Clamp((80f - num) / 80f, 0f, 1f));
		}
		else
		{
			combatCircle.SetAlpha(0f);
		}
	}

	public void CheckEngaged(bool applyEnchants)
	{
		newEngagedUnits.Clear();
		if ((!(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() != null) || !PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer()) && (!(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() == null) || !PandoraSingleton<Hermes>.Instance.IsHost()))
		{
			return;
		}
		if (!IsInFriendlyZone && unit.Status != UnitStateId.OUT_OF_ACTION)
		{
			List<UnitController> allEnemies = PandoraSingleton<MissionManager>.Instance.GetAllEnemies(unit.warbandIdx);
			Vector2 vector = default(Vector2);
			Vector2 vector2 = default(Vector2);
			for (int i = 0; i < allEnemies.Count; i++)
			{
				UnitController unitController = allEnemies[i];
				if (unitController.unit.Status == UnitStateId.OUT_OF_ACTION)
				{
					continue;
				}
				bool flag = false;
				Vector3 position = base.transform.position;
				float y = position.y;
				Vector3 position2 = unitController.transform.position;
				float num = Mathf.Abs(y - position2.y);
				if (num <= 1.9f && IsInRange(unitController, 0f, unit.ViewDistance, Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC), unitBlocking: false, checkAllBones: false, BoneId.NONE))
				{
					Vector3 position3 = base.transform.position;
					float x = position3.x;
					Vector3 position4 = base.transform.position;
					vector = new Vector2(x, position4.z);
					Vector3 position5 = unitController.transform.position;
					float x2 = position5.x;
					Vector3 position6 = unitController.transform.position;
					vector2 = new Vector2(x2, position6.z);
					float minEdgeDistance = (!EngagedUnits.Contains(unitController)) ? 0f : 0.1f;
					float d = 100f;
					Vector2 checkDestPoint = vector + (vector2 - vector) * d;
					flag = PandoraUtils.IsPointInsideEdges(unitController.combatCircle.Edges, vector, checkDestPoint, minEdgeDistance);
					if (!flag)
					{
						checkDestPoint = vector2 + (vector - vector2) * d;
						flag = PandoraUtils.IsPointInsideEdges(combatCircle.Edges, vector2, checkDestPoint, minEdgeDistance);
					}
				}
				if (flag)
				{
					newEngagedUnits.Add(unitController);
				}
			}
		}
		bool flag2 = newEngagedUnits.Count != EngagedUnits.Count;
		if (!flag2)
		{
			for (int j = 0; j < newEngagedUnits.Count; j++)
			{
				if (EngagedUnits.IndexOf(newEngagedUnits[j]) == -1)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			SendNewEngagedUnits(applyEnchants);
			ProcessEngagedUnits(applyEnchants);
		}
	}

	private void ProcessEngagedUnits(bool applyEnchants)
	{
		modifiedUnits.Clear();
		involvedUnits.Clear();
		modifiedUnits.Add(this);
		for (int i = 0; i < newEngagedUnits.Count; i++)
		{
			UnitController unitController = newEngagedUnits[i];
			if (!EngagedUnits.Contains(unitController))
			{
				PandoraUtils.InsertDistinct(ref modifiedUnits, unitController);
				EngagedUnits.Add(unitController);
				PandoraDebug.LogInfo("Adding engaged unit " + unitController.name + " to " + base.name);
			}
			if (!unitController.EngagedUnits.Contains(this))
			{
				unitController.EngagedUnits.Add(this);
				PandoraDebug.LogInfo("Adding engaged unit " + base.name + " to " + unitController.name);
			}
		}
		for (int num = EngagedUnits.Count - 1; num >= 0; num--)
		{
			UnitController unitController2 = EngagedUnits[num];
			if (!newEngagedUnits.Contains(unitController2))
			{
				EngagedUnits.RemoveAt(num);
				PandoraUtils.InsertDistinct(ref modifiedUnits, unitController2);
				PandoraDebug.LogInfo("Removing engaged unit " + unitController2.name + " from " + base.name);
				if (unitController2.unit.Status != UnitStateId.OUT_OF_ACTION && unitController2.EngagedUnits.Remove(this))
				{
					PandoraDebug.LogInfo("Removing engaged unit " + base.name + " from " + unitController2.name);
				}
			}
		}
		if (!applyEnchants)
		{
			return;
		}
		for (int j = 0; j < modifiedUnits.Count; j++)
		{
			modifiedUnits[j].TriggerEngagedEnchantments();
			PandoraUtils.InsertDistinct(ref involvedUnits, modifiedUnits[j]);
			for (int k = 0; k < modifiedUnits[j].EngagedUnits.Count; k++)
			{
				PandoraUtils.InsertDistinct(ref involvedUnits, modifiedUnits[j].EngagedUnits[k]);
			}
		}
		for (int l = 0; l < involvedUnits.Count; l++)
		{
			involvedUnits[l].TriggerAlliesEnchantments();
		}
	}

	public void TriggerEngagedEnchantments()
	{
		for (int num = unit.Enchantments.Count - 1; num >= 0; num--)
		{
			if (unit.Enchantments[num].Data.EnchantmentTriggerIdDestroy == EnchantmentTriggerId.ON_ENGAGED_OWNER && !IsEngagedUnit(unit.Enchantments[num].Provider))
			{
				unit.RemoveEnchantment(num);
			}
		}
		if (EngagedUnits.Count == 0)
		{
			PandoraDebug.LogInfo(base.name + " not engaged anymore", "ENGAGE", this);
			unit.DestroyEnchantments(EnchantmentTriggerId.ON_ENGAGED_SINGLE);
			unit.DestroyEnchantments(EnchantmentTriggerId.ON_ENGAGED_MULTIPLE);
		}
		else if (EngagedUnits.Count == 1)
		{
			PandoraDebug.LogInfo(base.name + " now engaged with single unit", "ENGAGE", this);
			unit.DestroyEnchantments(EnchantmentTriggerId.ON_ENGAGED_SINGLE);
			TriggerEnchantments(EnchantmentTriggerId.ON_ENGAGED_SINGLE);
		}
		else if (EngagedUnits.Count > 1)
		{
			PandoraDebug.LogInfo(base.name + " now engaged with multiple units", "ENGAGE", this);
			unit.DestroyEnchantments(EnchantmentTriggerId.ON_ENGAGED_MULTIPLE);
			TriggerEnchantments(EnchantmentTriggerId.ON_ENGAGED_MULTIPLE);
		}
	}

	private bool IsEngagedUnit(Unit unit)
	{
		for (int i = 0; i < EngagedUnits.Count; i++)
		{
			if (EngagedUnits[i].unit == unit)
			{
				return true;
			}
		}
		return false;
	}

	public void TriggerAlliesEnchantments()
	{
		int num = 0;
		int teamIdx = GetWarband().teamIdx;
		for (int i = 0; i < EngagedUnits.Count; i++)
		{
			if (num != 0)
			{
				break;
			}
			for (int j = 0; j < EngagedUnits[i].EngagedUnits.Count; j++)
			{
				if (num != 0)
				{
					break;
				}
				UnitController unitController = EngagedUnits[i].EngagedUnits[j];
				if (unitController != this && unitController.GetWarband().teamIdx == teamIdx)
				{
					num++;
				}
			}
		}
		PandoraDebug.LogInfo(base.name + " has " + num + " allies", "ENGAGE", this);
		unit.DestroyEnchantments((num != 0) ? EnchantmentTriggerId.ON_ENGAGED_ALLY : EnchantmentTriggerId.ON_ENGAGED_NO_ALLY);
		TriggerEnchantments((num != 0) ? EnchantmentTriggerId.ON_ENGAGED_ALLY : EnchantmentTriggerId.ON_ENGAGED_NO_ALLY);
	}

	public bool HasSpells()
	{
		for (int i = 0; i < actionStatus.Count; i++)
		{
			if (actionStatus[i].ActionId == UnitActionId.SPELL)
			{
				return true;
			}
		}
		return false;
	}

	public List<MonoBehaviour> GetCurrentActionTargets()
	{
		availableTargets.Clear();
		for (int i = 0; i < CurrentAction.Targets.Count; i++)
		{
			availableTargets.Add(CurrentAction.Targets[i]);
		}
		for (int j = 0; j < CurrentAction.Destructibles.Count; j++)
		{
			availableTargets.Add(CurrentAction.Destructibles[j]);
		}
		return availableTargets;
	}

	public void SetArcTargets(List<MonoBehaviour> targets, Vector3 dir, bool highlightTargets)
	{
		defenders.Clear();
		destructTargets.Clear();
		for (int i = 0; i < targets.Count; i++)
		{
			Vector3 to = targets[i].transform.position - base.transform.position;
			if (Vector3.Angle(dir, to) <= (float)currentAction.skillData.Angle / 2f)
			{
				if (targets[i] is UnitController)
				{
					defenders.Add((UnitController)targets[i]);
				}
				else if (targets[i] is Destructible)
				{
					destructTargets.Add((Destructible)targets[i]);
				}
			}
			if (highlightTargets)
			{
				HighlightTargets();
			}
		}
	}

	public void SetAoeTargets(List<MonoBehaviour> targets, Transform aoeSphere, bool highlightTargets)
	{
		float @float = Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC);
		defenders.Clear();
		destructTargets.Clear();
		Vector3 vector = base.transform.position + Vector3.up * 1.25f;
		Vector3 vector2 = vector + (aoeSphere.position - vector) * 0.99f;
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] is UnitController)
			{
				UnitController unitController = (UnitController)targets[i];
				TargetData targetData = new TargetData(unitController);
				if (CanTargetFromPoint(vector2, targetData, 0f, CurrentAction.Radius, @float, unitBlocking: false, checkAllBones: false))
				{
					defenders.Add(unitController);
				}
			}
			else if (targets[i] is Destructible)
			{
				Destructible destructible = (Destructible)targets[i];
				if (destructible.IsInRange(vector2, CurrentAction.Radius))
				{
					destructTargets.Add(destructible);
				}
			}
		}
		if (highlightTargets)
		{
			HighlightTargets();
		}
	}

	public void SetConeTargets(List<MonoBehaviour> targets, Transform cone, bool highlighTargets)
	{
		SetGeometryTargets(targets, cone, pointInsideCone, cone.forward, highlighTargets);
	}

	public void SetLineTargets(List<MonoBehaviour> targets, Transform line, bool highlighTargets)
	{
		SetGeometryTargets(targets, line, pointsInsideCylinder, line.forward, highlighTargets);
	}

	public void SetCylinderTargets(List<MonoBehaviour> targets, Transform cylinder, bool highlighTargets)
	{
		SetGeometryTargets(targets, cylinder, pointsInsideCylinder, cylinder.up, highlighTargets);
	}

	private void SetGeometryTargets(List<MonoBehaviour> targets, Transform geo, CheckGeometry geometryChecker, Vector3 dir, bool highlightTargets)
	{
		bool flag = CurrentAction.IsShootAction();
		float requiredPerc = (!flag) ? Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC) : Constant.GetFloat(ConstantId.RANGE_SHOOT_REQUIRED_PERC);
		defenders.Clear();
		destructTargets.Clear();
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] is UnitController)
			{
				UnitController unitController = (UnitController)targets[i];
				TargetData targetData = new TargetData(unitController);
				if (!CanTargetFromPoint(geo.position, targetData, CurrentAction.RangeMin, CurrentAction.RangeMax, requiredPerc, flag, flag))
				{
					continue;
				}
				bool flag2 = false;
				List<BoneTargetRange> list = (!flag) ? targetData.boneTargetRange : targetData.boneTargetRangeBlockingUnit;
				int num = 0;
				while (!flag2 && num < list.Count)
				{
					if (list[num].hitBone && geometryChecker(unitController.transform.TransformPoint(unitController.boneTargets[num].position), geo, CurrentAction.Radius, CurrentAction.RangeMax, dir))
					{
						flag2 = true;
						break;
					}
					num++;
				}
				if (flag2)
				{
					defenders.Add(unitController);
				}
			}
			else if (targets[i] is Destructible && ((Destructible)targets[i]).IsInRange(geo.position, CurrentAction.RangeMax) && geometryChecker(targets[i].transform.position + Vector3.up, geo, CurrentAction.Radius, CurrentAction.RangeMax, dir))
			{
				destructTargets.Add((Destructible)targets[i]);
			}
		}
		if (highlightTargets)
		{
			HighlightTargets();
		}
	}

	private bool pointInsideCone(Vector3 pos, Transform cone, float coneRadius, float coneRange, Vector3 dir)
	{
		float num = Vector3.Dot(pos - cone.position, dir);
		if (num >= 0f && num <= coneRange)
		{
			float num2 = num / coneRange * coneRadius;
			float num3 = Vector3.SqrMagnitude(pos - cone.position - dir * num);
			return num3 < num2 * num2;
		}
		return false;
	}

	private bool pointsInsideCylinder(Vector3 pos, Transform cylinder, float cylinderRadius, float cylinderRange, Vector3 dir)
	{
		return pointsInsideCylinder(pos, cylinder.position, cylinderRadius, cylinderRange, dir);
	}

	private bool pointsInsideCylinder(Vector3 pos, Vector3 cylinderPos, float cylinderRadius, float cylinderRange, Vector3 dir)
	{
		float num = Vector3.Dot(pos - cylinderPos, dir);
		if (num >= 0f && num <= cylinderRange)
		{
			float num2 = Vector3.SqrMagnitude(pos - cylinderPos - dir * num);
			return num2 < cylinderRadius * cylinderRadius;
		}
		return false;
	}

	public bool isInsideCylinder(Vector3 cyclinderPos, float radius, float height, Vector3 up)
	{
		for (int i = 0; i < boneTargets.Count; i++)
		{
			Vector3 pos = base.transform.TransformPoint(boneTargets[i].position);
			if (pointsInsideCylinder(pos, cyclinderPos, radius, height, up))
			{
				return true;
			}
		}
		return false;
	}

	private void HighlightTargets()
	{
		ClearFlyingTexts();
		for (int i = 0; i < defenders.Count; i++)
		{
			if (defenders[i].Imprint.State == MapImprintStateId.VISIBLE)
			{
				int idx = i;
				PandoraSingleton<FlyingTextManager>.Instance.GetFlyingText(FlyingTextId.TARGET, delegate(FlyingText flyingText)
				{
					FlyingTarget flyingTarget2 = (FlyingTarget)flyingText;
					flyingOverviews.Add(flyingTarget2);
					flyingTarget2.Play(this, defenders[idx]);
					defenders[idx].Highlight.On(Color.red);
				});
			}
		}
		for (int j = 0; j < destructTargets.Count; j++)
		{
			if (destructTargets[j].Imprint.State == MapImprintStateId.VISIBLE)
			{
				int idx2 = j;
				PandoraSingleton<FlyingTextManager>.Instance.GetFlyingText(FlyingTextId.TARGET, delegate(FlyingText flyingText)
				{
					FlyingTarget flyingTarget = (FlyingTarget)flyingText;
					flyingOverviews.Add(flyingTarget);
					flyingTarget.Play(this, destructTargets[idx2]);
					destructTargets[idx2].Highlight.On(Color.red);
				});
			}
		}
	}

	public void ClearFlyingTexts()
	{
		for (int i = 0; i < flyingOverviews.Count; i++)
		{
			flyingOverviews[i].Deactivate();
		}
		flyingOverviews.Clear();
	}

	public void HitDefenders(Vector3 projDir, bool hurt = true)
	{
		for (int i = 0; i < defenders.Count; i++)
		{
			UnitController unitController = defenders[i];
			unitController.unit.UpdateEnchantmentsFx();
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, unitController);
			if (unitController.unit.PreviousStatus == UnitStateId.STUNNED && unitController.unit.Status == UnitStateId.NONE)
			{
				hurt = true;
				unitController.attackResultId = AttackResultId.NONE;
			}
			if (unitController.unit.Status == UnitStateId.STUNNED)
			{
				hurt = true;
				unitController.attackResultId = AttackResultId.HIT;
			}
			if (hurt)
			{
				if (unitController.lastActionWounds <= 0)
				{
					int variation = 0;
					if (unitController.attackResultId == AttackResultId.HIT || unitController.attackResultId == AttackResultId.HIT_NO_WOUND)
					{
						Vector3 forward = unitController.transform.forward;
						forward.y = 0f;
						variation = ((unitController.unit.Status != UnitStateId.OUT_OF_ACTION) ? 1 : 0);
						variation += ((!(Vector3.Angle(projDir, forward) > 90f)) ? 1 : 0);
					}
					unitController.PlayDefState(unitController.attackResultId, variation, unitController.unit.Status);
				}
				else if (unitController.lastActionWounds > 0)
				{
					unitController.PlayBuffDebuff(EffectTypeId.BUFF);
					unitController.EventDisplayDamage();
				}
			}
			else if ((currentAction.skillData.EffectTypeId == EffectTypeId.BUFF || currentAction.skillData.EffectTypeId == EffectTypeId.DEBUFF) && unitController.unit.HasEnchantmentsChanged)
			{
				unitController.PlayBuffDebuff(currentAction.skillData.EffectTypeId);
			}
			if (CurrentAction.fxData != null)
			{
				PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(CurrentAction.fxData.HitFx, unitController, null, null);
			}
		}
	}

	public void Hit()
	{
		if (attackResultId != 0 || unit.Status != 0)
		{
			PlayDefState(attackResultId, (unit.Status == UnitStateId.OUT_OF_ACTION) ? 1 : 0, unit.Status);
		}
		else if (buffResultId != 0)
		{
			PlayBuffDebuff(buffResultId);
			buffResultId = EffectTypeId.NONE;
		}
	}

	public void EnterZoneAoeAnim()
	{
		if (attackResultId != 0 || unit.Status != 0)
		{
			PlayDefState(attackResultId, 0, unit.Status);
		}
		else
		{
			PlayBuffDebuff(currentZoneAoe.GetEnterEffectType());
		}
	}

	public void RefreshDetected()
	{
		base.Highlight.OccluderOn();
		float num = (float)unit.Movement * 2f;
		num *= num;
		for (int i = 0; i < detectedUnits.Count; i++)
		{
			UnitController unitController = detectedUnits[i];
			if (!(unitController != null))
			{
				continue;
			}
			if (Vector3.SqrMagnitude(base.transform.position - unitController.transform.position) < num)
			{
				unitController.Highlight.On(DETECTED_COLOR);
				unitController.Highlight.seeThrough = true;
				if (!unitController.Imprint.alwaysVisible)
				{
					unitController.Imprint.alwaysVisible = true;
					unitController.Imprint.needsRefresh = true;
				}
			}
			else
			{
				unitController.Highlight.Off();
				if (unitController.GetWarband().idx != GetWarband().idx && unitController.Imprint.alwaysVisible)
				{
					unitController.Imprint.alwaysVisible = false;
					unitController.Imprint.needsRefresh = true;
				}
			}
		}
		for (int j = 0; j < detectedTriggers.Count; j++)
		{
			if (detectedTriggers[j] != null)
			{
				if (Vector3.SqrMagnitude(base.transform.position - detectedTriggers[j].transform.position) < num)
				{
					detectedTriggers[j].Highlight.On(DETECTED_COLOR);
					detectedTriggers[j].Highlight.seeThrough = true;
				}
				else
				{
					detectedTriggers[j].Highlight.Off();
				}
			}
		}
		for (int k = 0; k < detectedInteractivePoints.Count; k++)
		{
			if (!(detectedInteractivePoints[k] != null))
			{
				continue;
			}
			if (Vector3.SqrMagnitude(base.transform.position - detectedInteractivePoints[k].transform.position) < num)
			{
				if ((UnityEngine.Object)(object)detectedInteractivePoints[k].Highlight != null)
				{
					detectedInteractivePoints[k].Highlight.On(DETECTED_COLOR);
					detectedInteractivePoints[k].Highlight.seeThrough = true;
				}
				if (detectedInteractivePoints[k].Imprint != null && !detectedInteractivePoints[k].Imprint.alwaysVisible)
				{
					detectedInteractivePoints[k].Imprint.alwaysVisible = true;
					detectedInteractivePoints[k].Imprint.needsRefresh = true;
				}
			}
			else if (detectedInteractivePoints[k].Imprint != null && detectedInteractivePoints[k].Imprint.alwaysVisible)
			{
				detectedInteractivePoints[k].Imprint.alwaysVisible = false;
				detectedInteractivePoints[k].Imprint.needsRefresh = true;
			}
		}
	}

	public void HideDetected()
	{
		for (int i = 0; i < detectedUnits.Count; i++)
		{
			if (detectedUnits[i] != null)
			{
				detectedUnits[i].Highlight.Off();
				detectedUnits[i].Highlight.seeThrough = false;
				if (detectedUnits[i].GetWarband().idx != GetWarband().idx)
				{
					detectedUnits[i].Imprint.alwaysVisible = false;
				}
				detectedUnits[i].Imprint.needsRefresh = true;
			}
		}
		for (int j = 0; j < detectedTriggers.Count; j++)
		{
			if (detectedTriggers[j] != null)
			{
				detectedTriggers[j].Highlight.Off();
				detectedTriggers[j].Highlight.seeThrough = false;
			}
		}
		for (int k = 0; k < detectedInteractivePoints.Count; k++)
		{
			if (detectedInteractivePoints[k] != null)
			{
				if ((UnityEngine.Object)(object)detectedInteractivePoints[k].Highlight != null)
				{
					detectedInteractivePoints[k].Highlight.Off();
					detectedInteractivePoints[k].Highlight.seeThrough = false;
				}
				if (detectedInteractivePoints[k].Imprint != null)
				{
					detectedInteractivePoints[k].Imprint.alwaysVisible = false;
					detectedInteractivePoints[k].Imprint.needsRefresh = true;
				}
			}
		}
	}

	private void Fade(bool fade)
	{
		if (!fade)
		{
			if (Imprint != null && Imprint.State == MapImprintStateId.VISIBLE)
			{
				Hide(hide: false);
			}
		}
		else
		{
			Hide(hide: true);
		}
	}

	public void ReduceAlliesNavCutterSize(Action alliesNavReducedCb)
	{
		StartCoroutine(ReduceNavCutterSize(alliesNavReducedCb));
	}

	private IEnumerator ReduceNavCutterSize(Action alliesNavReducedCb)
	{
		PandoraSingleton<MissionManager>.Instance.lockNavRefresh = true;
		List<UnitController> allies = PandoraSingleton<MissionManager>.Instance.GetAliveAllies(unit.warbandIdx);
		for (int i = 0; i < allies.Count; i++)
		{
			if (allies[i] != this)
			{
				if (allies[i].AICtrlr != null && allies[i].Engaged)
				{
					allies[i].combatCircle.SetNavCutter();
				}
				else
				{
					allies[i].combatCircle.OverrideNavCutterRadius(CapsuleRadius + allies[i].CapsuleRadius + 0.1f);
				}
				allies[i].SetGraphWalkability(walkable: false);
				yield return null;
			}
		}
		PandoraSingleton<MissionManager>.Instance.lockNavRefresh = false;
		while (PandoraSingleton<MissionManager>.Instance.IsNavmeshUpdating)
		{
			yield return null;
		}
		alliesNavReducedCb?.Invoke();
	}

	public void RestoreAlliesNavCutterSize()
	{
		List<UnitController> aliveAllies = PandoraSingleton<MissionManager>.Instance.GetAliveAllies(unit.warbandIdx);
		for (int i = 0; i < aliveAllies.Count; i++)
		{
			if (aliveAllies[i] != this)
			{
				aliveAllies[i].combatCircle.SetNavCutter();
				aliveAllies[i].SetGraphWalkability(walkable: false);
			}
		}
	}

	public bool isNavCutterActive()
	{
		return combatCircle.NavCutterEnabled;
	}

	public void SetGraphWalkability(bool walkable)
	{
		walkable |= (unit.Status == UnitStateId.OUT_OF_ACTION);
		if (!combatCircle.NavCutterEnabled && !walkable)
		{
			SetCombatCircle(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit());
		}
		combatCircle.SetNavCutterEnabled(!walkable);
		PandoraSingleton<MissionManager>.Instance.RefreshGraph();
	}

	public void ClampToNavMesh()
	{
		if (!IsFixed)
		{
			base.transform.position = PandoraSingleton<MissionManager>.Instance.ClampToNavMesh(base.transform.position);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.parent != null)
		{
			ZoneAoe component = other.transform.parent.gameObject.GetComponent<ZoneAoe>();
			if (component != null)
			{
				component.EnterZone(this);
			}
		}
		if (IsFixed)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		Beacon beacon = null;
		if (other.transform.parent != null)
		{
			beacon = other.transform.parent.gameObject.GetComponent<Beacon>();
		}
		if (beacon != null)
		{
			RevertBeacons(beacon);
			return;
		}
		if (!Engaged && other.transform.parent != null && other.transform.parent.parent != null)
		{
			UnitController component2 = other.transform.parent.parent.gameObject.GetComponent<UnitController>();
			if (component2 != null && component2.unit.Status != UnitStateId.OUT_OF_ACTION && component2.GetWarband().teamIdx == GetWarband().teamIdx && friendlyEntered.IndexOf(component2) == -1)
			{
				friendlyEntered.Add(component2);
				flag = true;
				flag2 = true;
				if (friendlyEntered.Count == 1)
				{
					float d = Constant.GetFloat((component2.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? ConstantId.MELEE_RANGE_NORMAL : ConstantId.MELEE_RANGE_LARGE) + 0.1f;
					friendlyZoneEntryPoint = component2.transform.position + Vector3.Normalize(base.transform.position - component2.transform.position) * d;
				}
			}
		}
		InteractivePoint[] components = other.GetComponents<InteractivePoint>();
		if (components.Length == 0 && other.transform.parent != null)
		{
			components = other.transform.parent.GetComponents<InteractivePoint>();
		}
		if (components.Length != 0)
		{
			foreach (InteractivePoint item in components)
			{
				if (interactivePoints.IndexOf(item) == -1)
				{
					interactivePoints.Add(item);
					flag = true;
				}
			}
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.INTERACTION_POINTS_CHANGED);
		}
		if (other.transform.parent != null)
		{
			LocateZone component3 = other.transform.parent.gameObject.GetComponent<LocateZone>();
			if (component3 != null)
			{
				GetWarband().LocateZone(component3);
			}
		}
		if (flag && Initialized && !PandoraSingleton<SequenceManager>.Instance.isPlaying)
		{
			UpdateActionStatus(notice: true, (!flag2) ? UnitActionRefreshId.ON_TRIGGER : UnitActionRefreshId.NONE);
		}
		if (StateMachine.GetActiveStateId() == 11)
		{
			((Moving)StateMachine.GetState(11)).OnTriggerEnter(other);
		}
		if (StateMachine.GetActiveStateId() == 17 || StateMachine.GetActiveStateId() == 18 || StateMachine.GetActiveStateId() == 47 || StateMachine.GetActiveStateId() == 48 || IsInFriendlyZone || !(other.transform.parent != null))
		{
			return;
		}
		TriggerPoint component4 = other.transform.parent.gameObject.GetComponent<TriggerPoint>();
		if (!(component4 != null))
		{
			return;
		}
		if (component4 is Destructible)
		{
			triggeredDestructibles.Add((Destructible)component4);
		}
		else
		{
			if (component4 is Trap && ((Trap)component4).TeamIdx == GetWarband().teamIdx)
			{
				return;
			}
			currentTeleporter = (component4 as Teleporter);
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.INTERACTION_POINTS_CHANGED);
			if (component4.IsActive())
			{
				if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
				{
					SendTriggerActivated(base.transform.position, base.transform.rotation, component4);
				}
				else
				{
					SetFixed(fix: true);
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform.parent != null)
		{
			ZoneAoe component = other.transform.parent.gameObject.GetComponent<ZoneAoe>();
			if (component != null)
			{
				component.ExitZone(this);
			}
		}
		if (IsFixed)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (other.transform.parent != null && other.transform.parent.parent != null)
		{
			UnitController component2 = other.transform.parent.parent.gameObject.GetComponent<UnitController>();
			if (component2 != null && component2.GetWarband().teamIdx == GetWarband().teamIdx && friendlyEntered.IndexOf(component2) != -1)
			{
				friendlyEntered.Remove(component2);
				flag = true;
				flag2 = true;
				if (!IsInFriendlyZone)
				{
					friendlyZoneEntryPoint = Vector3.zero;
				}
			}
		}
		InteractivePoint[] components = other.GetComponents<InteractivePoint>();
		if (components.Length == 0 && other.transform.parent != null)
		{
			components = other.transform.parent.GetComponents<InteractivePoint>();
		}
		if (components.Length != 0)
		{
			foreach (InteractivePoint item in components)
			{
				int num = interactivePoints.IndexOf(item);
				if (num != -1)
				{
					interactivePoints.RemoveAt(num);
					flag = true;
				}
			}
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.INTERACTION_POINTS_CHANGED);
		}
		if (flag && !PandoraSingleton<SequenceManager>.Instance.isPlaying)
		{
			UpdateActionStatus(notice: true, (!flag2) ? UnitActionRefreshId.ON_TRIGGER : UnitActionRefreshId.NONE);
		}
		if (StateMachine.GetActiveStateId() == 11)
		{
			((Moving)StateMachine.GetState(11)).OnTriggerExit(other);
		}
		if (!(other.transform.parent != null))
		{
			return;
		}
		TriggerPoint component3 = other.transform.parent.gameObject.GetComponent<TriggerPoint>();
		if (component3 != null)
		{
			if (component3 is Destructible)
			{
				triggeredDestructibles.Remove((Destructible)component3);
				return;
			}
			currentTeleporter = null;
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.INTERACTION_POINTS_CHANGED);
		}
	}

	public void StartSync()
	{
		PandoraDebug.LogDebug("start sync for unit ID " + uid + " Owner ID =" + owner, "HERMES", this);
		StartCoroutine("NetworkSync");
	}

	public void StopSync()
	{
		PandoraDebug.LogDebug("stop sync for unit ID " + uid + " Owner ID =" + owner, "HERMES", this);
		StopCoroutine("NetworkSync");
		Send(true, Hermes.SendTarget.OTHERS, uid, 1u, 0f, base.transform.rotation, base.transform.position);
	}

	private IEnumerator NetworkSync()
	{
		while (true)
		{
			SendSpeedPosition();
			yield return new WaitForSeconds(0.0166666675f);
		}
	}

	private void SendSpeedPosition()
	{
		Send(false, Hermes.SendTarget.OTHERS, uid, 1u, animator.GetFloat(AnimatorIds.speed), base.transform.rotation, base.transform.position);
	}

	private void NetworkSyncRPC(float speed, Quaternion rot, Vector3 pos)
	{
		base.transform.rotation = rot;
		base.transform.position = pos;
		if (IsFixed)
		{
			SetFixed(fix: true);
		}
		else
		{
			SetAnimSpeed((!(speed > 0f)) ? speed : (speed * 0.8f));
		}
	}

	public void SendMoveAndUpdateCircle(uint targetUID, Vector3 pos, Quaternion rot)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 18u, targetUID, pos, rot);
	}

	public void MoveAndUpdateCircleRPC(uint targetUID, Vector3 pos, Quaternion rot)
	{
		UnitController unitController = null;
		List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
		for (int i = 0; i < allUnits.Count; i++)
		{
			if (allUnits[i].uid == targetUID)
			{
				unitController = allUnits[i];
				break;
			}
		}
		unitController.transform.rotation = rot;
		unitController.transform.position = pos;
		if (unitController.IsFixed)
		{
			unitController.SetFixed(fix: true);
		}
		unitController.SetCombatCircle(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit());
		unitController.SetGraphWalkability(walkable: false);
		unitController.RemoveAthletics();
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.zoneAoes.Count; j++)
			{
				if (PandoraSingleton<MissionManager>.Instance.zoneAoes[j] != null)
				{
					PandoraSingleton<MissionManager>.Instance.zoneAoes[j].CheckEnterOrExitUnit(unitController, network: true);
				}
			}
		}
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(unitController);
	}

	public void SendFly()
	{
		Send(true, Hermes.SendTarget.ALL, uid, 21u);
		StateMachine.ChangeState(9);
	}

	public void FlyRPC()
	{
		StateMachine.ChangeState(48);
	}

	public void SendAthletic()
	{
		Send(true, Hermes.SendTarget.ALL, uid, 19u);
		StateMachine.ChangeState(9);
	}

	public void AthleticRPC()
	{
		StateMachine.ChangeState(47);
	}

	public void SendAthleticFinished(bool success, AthleticAction action)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 20u, success, (int)action);
		StateMachine.ChangeState(9);
	}

	public void AthleticFinishedRPC(bool success, int action)
	{
		if (success)
		{
			if (Engaged)
			{
				switch (action)
				{
				case 0:
					unit.AddEnchantment(EnchantmentId.ACTION_CLIMB_SUCCESS, unit, original: false);
					break;
				case 1:
					unit.AddEnchantment(EnchantmentId.ACTION_JUMP_DOWN_SUCCESS, unit, original: false);
					break;
				case 2:
					unit.AddEnchantment(EnchantmentId.ACTION_LEAP_SUCCESS, unit, original: false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				LaunchMelee(State.START_MOVE);
			}
			else
			{
				StateMachine.ChangeState(10);
			}
		}
		else if (Engaged)
		{
			LaunchMelee(State.ATHLETIC_COUNTER);
		}
		else
		{
			StateMachine.ChangeState(45);
		}
	}

	public void SendCurse()
	{
		Send(true, Hermes.SendTarget.ALL, uid, 16u);
		StateMachine.ChangeState(9);
	}

	public void CurseRPC()
	{
		if (unit.Status == UnitStateId.OUT_OF_ACTION)
		{
			if (currentSpellSuccess && currentSpellId != 0)
			{
				ZoneAoe.Spawn(this, GetAction(currentSpellId));
				currentSpellId = SkillId.NONE;
			}
			StateMachine.ChangeState(10);
		}
		else
		{
			StateMachine.ChangeState(29);
		}
	}

	public void SendSkill(SkillId skillId)
	{
		if (AICtrlr != null)
		{
			AICtrlr.UsedSkill(skillId);
		}
		Send(true, Hermes.SendTarget.ALL, uid, 2u, (int)skillId);
		StateMachine.ChangeState(9);
	}

	public void SkillRPC(int skillId)
	{
		PandoraDebug.LogDebug("Skill " + (SkillId)skillId, "HERMES", this);
		ValidMove();
		SetCurrentAction((SkillId)skillId);
		currentSpellTargetPosition = base.transform.position;
		CurrentAction.Activate();
	}

	public void SendSkillSingleTarget(SkillId skillId, UnitController unitCtrlr)
	{
		if (AICtrlr != null)
		{
			AICtrlr.UsedSkill(skillId, unitCtrlr);
		}
		Send(true, Hermes.SendTarget.ALL, uid, 3u, (int)skillId, unitCtrlr.uid);
		StateMachine.ChangeState(9);
	}

	public void SkillSingleTargetRPC(int skillId, uint targetUID)
	{
		PandoraDebug.LogDebug("SkillSingleTarget " + (SkillId)skillId + "TargetId = " + targetUID, "HERMES", this);
		ValidMove();
		SetCurrentAction((SkillId)skillId);
		defenderCtrlr = null;
		destructibleTarget = null;
		List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
		for (int i = 0; i < allUnits.Count; i++)
		{
			if (allUnits[i].uid == targetUID)
			{
				defenderCtrlr = allUnits[i];
				break;
			}
		}
		if (defenderCtrlr != this)
		{
			FaceTarget(defenderCtrlr.transform, force: true);
		}
		currentSpellTargetPosition = defenderCtrlr.transform.position;
		CurrentAction.Activate();
	}

	public void SendSkillTargets(SkillId skillId, Vector3 targetPos, Vector3 targetDir)
	{
		if (AICtrlr != null)
		{
			AICtrlr.UsedSkill(skillId);
		}
		Send(true, Hermes.SendTarget.ALL, uid, 4u, (int)skillId, targetPos, targetDir);
		StateMachine.ChangeState(9);
	}

	public void SkillTargetsRPC(int skillId, Vector3 targetPos, Vector3 targetDir)
	{
		PandoraDebug.LogDebug("SkillTargetsRPC " + (SkillId)skillId + " Targetpos = " + targetPos + " TargetDir = " + targetDir, "HERMES", this);
		ValidMove();
		SetCurrentAction((SkillId)skillId);
		currentSpellTargetPosition = targetPos;
		Quaternion rotation = Quaternion.LookRotation(targetDir);
		if (unit.Id != UnitId.MANTICORE && !Mathf.Approximately(targetDir.x, 0f) && !Mathf.Approximately(targetDir.z, 0f))
		{
			targetDir.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(targetDir);
		}
		if (PandoraSingleton<MissionManager>.Instance.dummyTargeter == null)
		{
			PandoraSingleton<MissionManager>.Instance.dummyTargeter = new GameObject("dummyTargeter");
		}
		PandoraSingleton<MissionManager>.Instance.dummyTargeter.transform.position = targetPos;
		PandoraSingleton<MissionManager>.Instance.dummyTargeter.transform.rotation = rotation;
		UpdateTargetsData();
		CurrentAction.SetTargets();
		switch (CurrentAction.TargetingId)
		{
		case TargetingId.AREA:
			SetAoeTargets(GetCurrentActionTargets(), PandoraSingleton<MissionManager>.Instance.dummyTargeter.transform, highlightTargets: false);
			break;
		case TargetingId.AREA_GROUND:
			SetCylinderTargets(GetCurrentActionTargets(), PandoraSingleton<MissionManager>.Instance.dummyTargeter.transform, highlighTargets: false);
			break;
		case TargetingId.LINE:
			currentSpellTargetPosition += targetDir * CurrentAction.skillData.Range;
			SetLineTargets(GetCurrentActionTargets(), PandoraSingleton<MissionManager>.Instance.dummyTargeter.transform, highlighTargets: false);
			break;
		case TargetingId.CONE:
			currentSpellTargetPosition += targetDir * CurrentAction.skillData.Range;
			SetConeTargets(GetCurrentActionTargets(), PandoraSingleton<MissionManager>.Instance.dummyTargeter.transform, highlighTargets: false);
			break;
		case TargetingId.ARC:
			SetArcTargets(GetCurrentActionTargets(), targetDir, highlightTargets: false);
			break;
		}
		CurrentAction.Activate();
	}

	public void SendSkillSingleDestructible(SkillId skillId, Destructible destruct)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 23u, (int)skillId, destruct.guid);
		StateMachine.ChangeState(9);
	}

	public void SkillSingleDestructibleRPC(int skillId, uint destructUID)
	{
		PandoraDebug.LogDebug("SkillSingleTarget " + (SkillId)skillId + "TargetId = " + destructUID, "HERMES", this);
		ValidMove();
		SetCurrentAction((SkillId)skillId);
		defenderCtrlr = null;
		destructibleTarget = null;
		List<TriggerPoint> triggerPoints = PandoraSingleton<MissionManager>.Instance.triggerPoints;
		for (int i = 0; i < triggerPoints.Count; i++)
		{
			if (triggerPoints[i].guid == destructUID)
			{
				destructibleTarget = (Destructible)triggerPoints[i];
				break;
			}
		}
		FaceTarget(destructibleTarget.transform, force: true);
		currentSpellTargetPosition = destructibleTarget.transform.position;
		CurrentAction.Activate();
	}

	public void SendStartMove(Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 12u, currentUnitPos, currentUnitRot);
		StateMachine.ChangeState(9);
	}

	public void StartMoveRPC(Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		base.transform.position = currentUnitPos;
		base.transform.rotation = currentUnitRot;
		SetAnimSpeed(0f);
		SetFixed(fix: true);
		StateMachine.ChangeState(10);
	}

	public void SendTriggerActivated(Vector3 unitPos, Quaternion unitRot, TriggerPoint trigger)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 13u, unitPos, unitRot, PandoraSingleton<MissionManager>.Instance.triggerPoints.IndexOf(trigger));
	}

	public void TriggerActivatedRPC(Vector3 unitPos, Quaternion unitRot, int triggerIdx)
	{
		base.transform.position = unitPos;
		base.transform.rotation = unitRot;
		SetAnimSpeed(0f);
		SetFixed(fix: true);
		activeTrigger = PandoraSingleton<MissionManager>.Instance.triggerPoints[triggerIdx];
		nextState = ((!(activeTrigger is Trap)) ? State.TELEPORT : State.TRAPPED);
	}

	public void SendZoneAoeCross(ZoneAoe zone, bool entering, bool network)
	{
		int num = PandoraSingleton<MissionManager>.Instance.ZoneAoeIdx(zone);
		PandoraDebug.LogDebug("SendZoneAoeCross Zone = " + num + " entering = " + entering + " network = " + network, "HERMES", this);
		network &= PandoraSingleton<MissionManager>.Instance.transitionDone;
		if (network)
		{
			Send(true, Hermes.SendTarget.ALL, uid, 15u, num, entering);
		}
		else
		{
			ZoneAoeCrossRPC(num, entering);
		}
	}

	public void ZoneAoeCrossRPC(int zoneAoeIdx, bool entering)
	{
		PandoraDebug.LogDebug("ZoneAoeCrossRPC Zone = " + zoneAoeIdx + " entering = " + entering, "HERMES", this);
		currentZoneAoe = PandoraSingleton<MissionManager>.Instance.GetZoneAoe(zoneAoeIdx);
		UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
		if (entering)
		{
			currentZoneAoe.AddToAffected(this);
			currentZoneAoe.ApplyEnchantments(this, entry: true);
			lastActionWounds = 0;
			flyingLabel = string.Empty;
			if (PandoraSingleton<MissionManager>.Instance.transitionDone)
			{
				int enchantmentDamage = unit.GetEnchantmentDamage(PandoraSingleton<MissionManager>.Instance.NetworkTyche, EnchantmentDmgTriggerId.ON_APPLY);
				if (!PandoraSingleton<SequenceManager>.Instance.isPlaying)
				{
					PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.COMBAT_RIGHT_MESSAGE, "skill_name_" + currentZoneAoe.Name);
					PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.HideWithTimer();
				}
				bool flag = StateMachine.GetActiveStateId() == 38;
				bool flag2 = StateMachine.GetActiveStateId() == 41;
				unit.UpdateAttributes();
				attackerCtrlr = currentZoneAoe.Owner;
				if (enchantmentDamage > 0)
				{
					ComputeDirectWound(enchantmentDamage, byPassArmor: true, currentZoneAoe.Owner);
					if (unit.Status != UnitStateId.OUT_OF_ACTION)
					{
						if (IsFixed && PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() != this)
						{
							PlayDefState(AttackResultId.HIT, 0, unit.Status);
						}
						else
						{
							EventDisplayDamage();
						}
					}
					else
					{
						KillUnit();
						PlayDefState(AttackResultId.HIT, 0, UnitStateId.OUT_OF_ACTION);
						if (!PandoraSingleton<MissionManager>.Instance.CheckEndGame())
						{
							if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() && (this == currentUnit || flag || flag2))
							{
								SendStartMove(base.transform.position, base.transform.rotation);
								return;
							}
							if (this != currentUnit)
							{
								StateMachine.ChangeState(9);
							}
						}
					}
				}
				else if (unit.Status == UnitStateId.STUNNED)
				{
					PlayDefState(AttackResultId.HIT, 0, unit.Status);
					if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() && (this == currentUnit || flag || flag2) && !PandoraSingleton<SequenceManager>.Instance.isPlaying)
					{
						SendStartMove(base.transform.position, base.transform.rotation);
						return;
					}
				}
			}
			unit.UpdateAttributes();
		}
		else
		{
			currentZoneAoe.RemoveUnit(this, sendTrigger: false);
			currentZoneAoe.ApplyEnchantments(this, entry: false);
			unit.UpdateAttributes();
			currentZoneAoe = null;
		}
	}

	private void SendNewEngagedUnits(bool applyEnchants)
	{
		uint[] array = new uint[newEngagedUnits.Count];
		for (int i = 0; i < newEngagedUnits.Count; i++)
		{
			array[i] = newEngagedUnits[i].uid;
		}
		Send(true, Hermes.SendTarget.OTHERS, uid, 22u, array, applyEnchants, wasEngaged);
	}

	private void NewEngagedUnitsRPC(uint[] engagedUids, bool applyEnchants, bool previouslyEngaged)
	{
		wasEngaged = previouslyEngaged;
		newEngagedUnits.Clear();
		for (int i = 0; i < engagedUids.Length; i++)
		{
			newEngagedUnits.Add(PandoraSingleton<MissionManager>.Instance.GetUnitController(engagedUids[i]));
		}
		ProcessEngagedUnits(applyEnchants);
	}

	public void SendEngaged(Vector3 currentUnitPos, Quaternion currentUnitRot, bool charge = false)
	{
		PandoraDebug.LogDebug("Sending EngagedRPC  Charge = " + charge + " pos = " + currentUnitPos + " rot = " + currentUnitRot, "HERMES", this);
		Send(true, Hermes.SendTarget.ALL, uid, 11u, currentUnitPos, currentUnitRot, charge);
		StateMachine.ChangeState(9);
	}

	public void EngagedRPC(Vector3 currentUnitPos, Quaternion currentUnitRot, bool charge)
	{
		PandoraDebug.LogDebug("EngagedRPC 1 Charge = " + charge + " pos = " + currentUnitPos + " rot = " + currentUnitRot, "HERMES", this);
		base.transform.position = currentUnitPos;
		base.transform.rotation = currentUnitRot;
		SetAnimSpeed(0f);
		SetFixed(fix: true);
		if (!charge)
		{
			ValidMove();
		}
		friendlyEntered.Clear();
		PandoraDebug.LogDebug("EngagedRPC Engaged Units = " + EngagedUnits.Count + " pos = " + base.transform.position + " rot = " + base.transform.rotation, "HERMES", this);
		if (charge)
		{
			defenderCtrlr = EngagedUnits[0];
			FaceTarget(defenderCtrlr.transform, force: true);
			StateMachine.ChangeState(32);
		}
		else if (IsMine())
		{
			if (!wasEngaged)
			{
				wasEngaged = true;
				LaunchMelee((AICtrlr == null) ? State.ENGAGED : State.AI_CONTROLLED);
			}
			else
			{
				StateMachine.ChangeState((AICtrlr == null) ? 12 : 42);
			}
		}
		else if (!wasEngaged)
		{
			wasEngaged = true;
			LaunchMelee(State.NET_CONTROLLED);
		}
		else
		{
			StateMachine.ChangeState(43);
		}
	}

	public void SendAskInterruption(UnitActionId actionId, uint senderUID, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		PandoraDebug.LogInfo("SendAskInterruption Action" + actionId + " targetID = " + senderUID + " Position = " + currentUnitPos, "HERMES", this);
		Send(true, Hermes.SendTarget.HOST, uid, 17u, (int)actionId, senderUID, currentUnitPos, currentUnitRot);
	}

	private void AskInterruption(UnitActionId actionId, uint senderUID, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		if (PandoraSingleton<MissionManager>.Instance.interruptingUnit == null)
		{
			PandoraDebug.LogInfo("Unit " + base.name + " is interruption Action" + actionId + " targetID = " + senderUID + " Position = " + currentUnitPos, "HERMES", this);
			PandoraSingleton<MissionManager>.Instance.interruptingUnit = this;
			switch (actionId)
			{
			case UnitActionId.OVERWATCH:
				SendOverwatch(senderUID, currentUnitPos, currentUnitRot);
				break;
			case UnitActionId.AMBUSH:
				SendAmbush(senderUID, currentUnitPos, currentUnitRot);
				break;
			default:
				PandoraDebug.LogError("Action " + actionId + " not supported for interruption!", "HERMES", this);
				break;
			}
		}
		else
		{
			PandoraDebug.LogInfo("Unit " + base.name + " tried to interrupt the flow but " + PandoraSingleton<MissionManager>.Instance.interruptingUnit.name + " is already interrupting", "HERMES", this);
		}
	}

	private void SendOverwatch(uint senderUID, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		PandoraDebug.LogInfo("SendOverwatch ID = " + senderUID + " Position = " + currentUnitPos, "HERMES", this);
		Send(true, Hermes.SendTarget.ALL, uid, 5u, senderUID, currentUnitPos, currentUnitRot);
		StateMachine.ChangeState(9);
	}

	public void OverwatchRPC(uint senderUID, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
		int num = 0;
		while (true)
		{
			if (num < allUnits.Count)
			{
				if (allUnits[num].uid == senderUID)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		allUnits[num].OverwatchRPC(this, currentUnitPos, currentUnitRot);
	}

	public void OverwatchRPC(UnitController target, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		PandoraDebug.LogInfo("OverwatchRPC", "HERMES", this);
		defenderCtrlr = target;
		destructibleTarget = null;
		defenderCtrlr.SetAnimSpeed(0f);
		defenderCtrlr.transform.position = currentUnitPos;
		defenderCtrlr.transform.rotation = currentUnitRot;
		defenderCtrlr.SetFixed(fix: true);
		defenderCtrlr.ValidMove();
		defenderCtrlr.CurrentAction = null;
		unit.ConsumeEnchantments(EnchantmentConsumeId.OVERWATCH);
		unit.ConsumeEnchantments(EnchantmentConsumeId.AMBUSH);
		defenderCtrlr.WaitForAction(State.START_MOVE);
		yieldedPos = currentUnitPos;
		yieldedRot = currentUnitRot;
		yieldedSkillId = SkillId.BASE_OVERWATCH_ATTACK;
		StartCoroutine(LaunchYieldedAction());
	}

	private void SendAmbush(uint senderUID, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		PandoraDebug.LogInfo("Send AmbushRPC", "HERMES", this);
		Send(true, Hermes.SendTarget.ALL, uid, 6u, senderUID, currentUnitPos, currentUnitRot);
		StateMachine.ChangeState(9);
	}

	public void AmbushRPC(uint targetUID, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		List<UnitController> aliveEnemies = PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(GetWarband().idx);
		int num = 0;
		while (true)
		{
			if (num < aliveEnemies.Count)
			{
				if (aliveEnemies[num].uid == targetUID)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		aliveEnemies[num].AmbushRPC(this, currentUnitPos, currentUnitRot);
	}

	public void AmbushRPC(UnitController target, Vector3 currentUnitPos, Quaternion currentUnitRot)
	{
		PandoraDebug.LogInfo("AmbushRPC", "HERMES", this);
		defenderCtrlr = target;
		destructibleTarget = null;
		defenderCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_AMBUSHED);
		PandoraSingleton<MissionManager>.Instance.HideCombatCircles();
		defenderCtrlr.transform.position = currentUnitPos;
		defenderCtrlr.transform.rotation = currentUnitRot;
		defenderCtrlr.SetAnimSpeed(0f);
		defenderCtrlr.SetFixed(fix: true);
		defenderCtrlr.ValidMove();
		defenderCtrlr.CurrentAction = null;
		unit.ConsumeEnchantments(EnchantmentConsumeId.AMBUSH);
		defenderCtrlr.WaitForAction(State.START_MOVE);
		yieldedPos = currentUnitPos;
		yieldedRot = currentUnitRot;
		yieldedSkillId = SkillId.BASE_AMBUSH_ATTACK;
		StartCoroutine(LaunchYieldedAction());
	}

	private IEnumerator LaunchYieldedAction()
	{
		yield return new WaitForFixedUpdate();
		defenderCtrlr.SetAnimSpeed(0f);
		defenderCtrlr.transform.position = yieldedPos;
		defenderCtrlr.transform.rotation = yieldedRot;
		defenderCtrlr.SetFixed(fix: true);
		PandoraDebug.LogInfo("LaunchYieldedAction " + yieldedSkillId + " Position 3 = " + defenderCtrlr.transform.position, "HERMES", this);
		PandoraDebug.LogInfo("LaunchYieldedAction " + yieldedSkillId + " Rotation 3 = " + defenderCtrlr.transform.rotation, "HERMES", this);
		defenderCtrlr.SetCombatCircle(this, forced: true);
		SetCombatCircle(this);
		while (PandoraSingleton<MissionManager>.Instance.IsNavmeshUpdating)
		{
			yield return null;
		}
		SetCurrentAction(yieldedSkillId);
		CurrentAction.Activate();
	}

	public void SendInventoryTakeAll()
	{
		Send(true, Hermes.SendTarget.ALL, uid, 14u);
	}

	public void InventoryTakeAllRPC()
	{
		PandoraDebug.LogInfo("InventoryChangRPC", "HERMES", this);
		SearchPoint searchPoint = (SearchPoint)interactivePoint;
		Item.SortEmptyItems(searchPoint.items, 0);
		Item switchItem = new Item(ItemId.NONE);
		UnitSlotId slotId;
		while (searchPoint.items.Count > 0 && searchPoint.items[0].Id != 0 && searchPoint.CanSwitchItem(0, switchItem) && unit.GetEmptyItemSlot(out slotId, searchPoint.items[0]))
		{
			PandoraSingleton<MissionManager>.Instance.focusedUnit.InventoryChangeRPC(0, (int)(slotId - 6));
		}
		InventoryDoneRPC();
	}

	public void SendInventoryChange(int itemIndex, int slotIndex)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 7u, itemIndex, slotIndex);
	}

	public void InventoryChangeRPC(int itemIndex, int slotIndex)
	{
		PandoraDebug.LogInfo("InventoryChangeRPC - " + itemIndex + " - " + slotIndex, "HERMES", this);
		Inventory inventory = (Inventory)StateMachine.GetState(15);
		inventory.PickupItem(itemIndex, slotIndex);
	}

	public void SendInventoryDone()
	{
		Send(true, Hermes.SendTarget.ALL, uid, 8u);
	}

	public void InventoryDoneRPC()
	{
		PandoraDebug.LogInfo("InventoryDoneRPC", "HERMES", this);
		SearchPoint searchPoint = (SearchPoint)interactivePoint;
		searchPoint.Close();
		PandoraSingleton<MissionManager>.Instance.UpdateObjectivesUI();
		if (IsPlayed() && searchPoint.unitController != null && !searchPoint.unitController.IsPlayed() && PandoraSingleton<MissionManager>.Instance.lootedEnemies.IndexOf(searchPoint) == -1)
		{
			PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.LOOT_ENEMIES, 1);
			PandoraSingleton<MissionManager>.Instance.lootedEnemies.Add(searchPoint);
		}
		if (!PandoraSingleton<MissionManager>.Instance.CheckEndGame())
		{
			PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(this);
			if (!PandoraSingleton<MissionManager>.Instance.MissionEndData.missionSave.isTuto && PandoraSingleton<GameManager>.Instance.currentSave != null)
			{
				PandoraSingleton<MissionManager>.Instance.MissionEndData.wagonItems.Clear();
				PandoraSingleton<MissionManager>.Instance.MissionEndData.wagonItems.AddItems(PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().wagon.chest.GetItems());
			}
			StartCoroutine(WaitForSearchIdle());
		}
	}

	private IEnumerator WaitForSearchIdle()
	{
		while (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != AnimatorIds.search_idle && animator.GetCurrentAnimatorStateInfo(0).fullPathHash != AnimatorIds.interact_idle)
		{
			yield return 0;
		}
		PandoraSingleton<MissionManager>.Instance.PlaySequence("search", this, OnInvSeqDone);
	}

	private void OnInvSeqDone()
	{
		wyrdstoneRollModifier = 0;
		for (int i = 6; i < unit.Items.Count; i++)
		{
			Item item = unit.Items[i];
			if (!oldItems.Contains(item))
			{
				ItemId id = item.Id;
				if (id == ItemId.WYRDSTONE_FRAGMENT || id == ItemId.WYRDSTONE_SHARD || id == ItemId.WYRDSTONE_CLUSTER)
				{
					wyrdstoneRollModifier += PandoraSingleton<DataFactory>.Instance.InitData<ItemAttributeData>(new string[2]
					{
						"fk_item_id",
						"fk_attribute_id"
					}, new string[2]
					{
						((int)id).ToConstantString(),
						55.ToConstantString()
					})[0].Modifier;
				}
			}
		}
		UnitController unitController = ((SearchPoint)interactivePoint).SpawnCampaignUnit();
		((SearchPoint)interactivePoint).ActivateZoneAoe();
		if (unitController != null && unitController.unit.Id == UnitId.MANTICORE)
		{
			unitController.transform.position = Vector3.one * 10000f;
			if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
			{
				unitController.SendSkill(SkillId.BASE_FLY);
			}
			WaitForAction(State.START_MOVE);
		}
		else if (wyrdstoneRollModifier != 0)
		{
			TriggerEnchantments(EnchantmentTriggerId.ON_WYRDSTONE_PICKUP);
			currentSpellTypeId = SpellTypeId.WYRDSTONE;
			if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
			{
				SendCurse();
			}
			else
			{
				StateMachine.ChangeState(9);
			}
		}
		else if (((SearchPoint)interactivePoint).ShouldTriggerCurse())
		{
			currentSpellTypeId = SpellTypeId.MISSION;
			currentCurseSkillId = interactivePoint.curseId;
			if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
			{
				SendCurse();
			}
			else
			{
				StateMachine.ChangeState(9);
			}
		}
		else if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			SendStartMove(base.transform.position, base.transform.rotation);
		}
		else
		{
			StateMachine.ChangeState(9);
		}
	}

	public void SendInteractiveAction(SkillId skillId, InteractivePoint point)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 9u, skillId, PandoraSingleton<MissionManager>.Instance.GetInteractivePointIndex(point));
		StateMachine.ChangeState(9);
	}

	public void InteractiveRPC(int skillId, int actionZoneIdx)
	{
		PandoraDebug.LogInfo("AthleticRPC skillId = " + (SkillId)skillId + "Action zone index " + actionZoneIdx, "HERMES", this);
		interactivePoint = PandoraSingleton<MissionManager>.Instance.GetInteractivePoint(actionZoneIdx);
		SetCurrentAction((SkillId)skillId);
		ValidMove();
		activeActionDest = null;
		switch (CurrentAction.ActionId)
		{
		case UnitActionId.CLIMB:
			activeActionDest = ((ActionZone)interactivePoint).GetClimb();
			break;
		case UnitActionId.JUMP:
			activeActionDest = ((ActionZone)interactivePoint).GetJump();
			break;
		case UnitActionId.LEAP:
			activeActionDest = ((ActionZone)interactivePoint).GetLeap();
			break;
		}
		if (activeActionDest != null)
		{
			FaceTarget(activeActionDest.destination.transform.position, force: true);
		}
		CurrentAction.Activate();
	}

	public void SendActionDone()
	{
		Send(true, Hermes.SendTarget.ALL, uid, 10u);
		StateMachine.ChangeState(9);
	}

	private void ActionDoneRPC()
	{
		PandoraDebug.LogInfo("ActionDoneRPC", "HERMES", this);
		unit.UpdateValidNextActionEnchantments();
		ActionDone();
	}

	public void EventSheathe()
	{
		if (Sheated || currentActionId == UnitActionId.SWITCH_WEAPONSET)
		{
			SwitchWeapons((!Sheated) ? unit.InactiveWeaponSlot : unit.ActiveWeaponSlot);
			Sheated = false;
			return;
		}
		if (base.Equipments[(int)unit.ActiveWeaponSlot] != null)
		{
			base.Equipments[(int)unit.ActiveWeaponSlot].gameObject.SetActive(value: true);
			base.Equipments[(int)unit.ActiveWeaponSlot].Sheathe(base.BonesTr, offhand: false, unit.Id);
		}
		if (base.Equipments[(int)(unit.ActiveWeaponSlot + 1)] != null)
		{
			base.Equipments[(int)(unit.ActiveWeaponSlot + 1)].gameObject.SetActive(value: true);
			base.Equipments[(int)(unit.ActiveWeaponSlot + 1)].Sheathe(base.BonesTr, offhand: true, unit.Id);
		}
		unit.currentAnimStyleId = AnimStyleId.NONE;
		SetAnimStyle();
		Sheated = true;
	}

	public void EventHurt(int variation)
	{
		for (int i = 0; i < defenders.Count; i++)
		{
			if (defenders[i].attackResultId == AttackResultId.HIT_NO_WOUND || defenders[i].attackResultId == AttackResultId.HIT)
			{
				defenders[i].PlayDefState(defenders[i].attackResultId, variation, defenders[i].unit.Status);
				if (CurrentAction.fxData != null)
				{
					PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(CurrentAction.fxData.ImpactFx, defenders[i], null, null);
				}
				if (criticalHit && (IsPlayed() || defenderCtrlr.IsPlayed()))
				{
					PandoraSingleton<MissionManager>.Instance.CamManager.ActivateBloodSplatter();
				}
			}
		}
		for (int j = 0; j < destructTargets.Count; j++)
		{
			destructTargets[j].Hit(this);
		}
	}

	public void EventAvoid(int variation)
	{
		if (defenderCtrlr == null)
		{
			return;
		}
		for (int i = 0; i < defenders.Count; i++)
		{
			if ((defenders[i].attackResultId == AttackResultId.MISS || defenders[i].attackResultId == AttackResultId.DODGE) && defenders[i].unit.Status == UnitStateId.NONE)
			{
				defenders[i].PlayDefState(defenders[i].attackResultId, variation, defenders[i].unit.Status);
			}
		}
	}

	public void EventParry()
	{
		if (defenderCtrlr == null)
		{
			return;
		}
		for (int i = 0; i < defenders.Count; i++)
		{
			if (defenders[i].attackResultId == AttackResultId.PARRY)
			{
				defenders[i].PlayDefState(defenders[i].attackResultId, 0, defenders[i].unit.Status);
			}
		}
	}

	public void EventFx(string fxName)
	{
		PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(fxName, this, null, null);
	}

	public void EventSoundFoot(string soundName)
	{
		GetRandomSound(ref soundName, ref lastFoot);
		PlaySound(soundName);
	}

	public void EventShout(string soundName)
	{
		GetParamSound(ref soundName);
		GetRandomSound(ref soundName, ref lastShout);
		PlaySound(soundName);
	}

	public void EventSound(string soundName)
	{
		GetParamSound(ref soundName);
		PlaySound(soundName);
	}

	private void GetParamSound(ref string soundName)
	{
		while (soundName.IndexOf("(") != -1)
		{
			int num = soundName.IndexOf("(");
			int num2 = soundName.IndexOf(")");
			string text = soundName.Substring(num + 1, num2 - num - 1);
			string text2 = string.Empty;
			switch (text)
			{
			case "armor":
				text2 = unit.Items[1].Sound;
				text2 = ((!string.IsNullOrEmpty(text2)) ? text2 : "cloth");
				break;
			case "weapon":
				text2 = unit.Items[(int)unit.ActiveWeaponSlot].Sound;
				break;
			case "off_weapon":
				text2 = unit.Items[(int)(unit.ActiveWeaponSlot + 1)].Sound;
				break;
			case "atk_weapon":
				if (attackerCtrlr != null && attackerCtrlr.HasClose())
				{
					text2 = attackerCtrlr.unit.Items[(int)attackerCtrlr.unit.ActiveWeaponSlot].Sound;
				}
				break;
			case "atk_weapon_cat":
				if (attackerCtrlr != null && attackerCtrlr.HasClose())
				{
					text2 = attackerCtrlr.unit.Items[(int)attackerCtrlr.unit.ActiveWeaponSlot].SoundCat;
				}
				break;
			case "atk_proj":
				if (attackerCtrlr != null && attackerCtrlr.HasRange())
				{
					text2 = attackerCtrlr.unit.Items[(int)attackerCtrlr.unit.ActiveWeaponSlot].ProjectileData.Sound;
				}
				break;
			case "unit":
				text2 = unit.Data.Name;
				break;
			case "spell":
				text2 = CurrentAction.skillData.Name;
				break;
			case "atk_spell":
				if (PandoraSingleton<MissionManager>.Instance.focusedUnit != null && PandoraSingleton<MissionManager>.Instance.focusedUnit.CurrentAction != null)
				{
					text2 = PandoraSingleton<MissionManager>.Instance.focusedUnit.CurrentAction.skillData.Name;
				}
				break;
			case "skill":
				if (CurrentAction != null && CurrentAction.SkillId != 0)
				{
					text2 = CurrentAction.skillData.Name;
					text2 = text2.Replace("_mstr", string.Empty);
				}
				break;
			}
			if (text2 == string.Empty)
			{
				break;
			}
			StringBuilder stringBuilder = PandoraUtils.StringBuilder;
			stringBuilder.Append(soundName.Substring(0, num));
			stringBuilder.Append(text2.ToLower());
			stringBuilder.Append(soundName.Substring(num2 + 1, soundName.Length - num2 - 1));
			soundName = stringBuilder.ToString();
		}
	}

	private void GetRandomSound(ref string soundName, ref string lastSoundPlayed)
	{
		int num = int.Parse(soundName.Substring(soundName.Length - 1));
		string arg = soundName.Substring(0, soundName.Length - 1);
		int num2 = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, num + 1);
		string a = arg + num2;
		if (a == lastSoundPlayed)
		{
			num2 = ++num2 % (num + 1);
			if (num2 == 0)
			{
				num2++;
			}
		}
		lastSoundPlayed = arg + num2;
		soundName = lastSoundPlayed;
	}

	private void PlaySound(string soundName)
	{
		if (!(audioSource == null))
		{
			PandoraSingleton<Pan>.Instance.GetSound(soundName, cache: true, delegate(AudioClip clip)
			{
				if (clip != null)
				{
					audioSource.PlayOneShot(clip);
				}
			});
		}
	}

	public void EventDisplayDamage()
	{
		if (!string.IsNullOrEmpty(flyingLabel) && Imprint.State == MapImprintStateId.VISIBLE)
		{
			PandoraSingleton<FlyingTextManager>.Instance.GetFlyingText((!(attackerCtrlr != null) || !attackerCtrlr.criticalHit) ? FlyingTextId.DAMAGE : FlyingTextId.DAMAGE_CRIT, delegate(FlyingText fl)
			{
				((FlyingLabel)fl).Play(base.BonesTr[BoneId.RIG_HEAD].localPosition, base.transform, false, flyingLabel);
			});
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, this);
		}
	}

	public void EventDisplayActionOutcome()
	{
		if (!string.IsNullOrEmpty(currentActionData.actionOutcome) && Imprint.State == MapImprintStateId.VISIBLE && PandoraSingleton<TransitionManager>.Instance.GameLoadingDone)
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, this);
		}
	}

	public void EventDisplayStatusOutcome()
	{
		if (unit.Status != 0 && unit.Status != unit.PreviousStatus && Imprint.State == MapImprintStateId.VISIBLE)
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_STATUS, this, PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("unit_state_", unit.Status.ToString()));
		}
	}

	public void EventTrail(int active)
	{
		bool activate = active != 0;
		string newColor = (currentAction.fxData == null) ? string.Empty : currentAction.fxData.TrailColor;
		if (base.Equipments[(int)unit.ActiveWeaponSlot] != null)
		{
			ActivateTrails(base.Equipments[(int)unit.ActiveWeaponSlot].trails, activate, newColor);
		}
		if (base.Equipments[(int)(unit.ActiveWeaponSlot + 1)] != null)
		{
			ActivateTrails(base.Equipments[(int)(unit.ActiveWeaponSlot + 1)].trails, activate, newColor);
		}
		ActivateTrails(bodyPartTrails, activate, newColor);
	}

	private void ActivateTrails(List<WeaponTrail> trails, bool activate, string newColor)
	{
		if (trails == null || trails.Count <= 0)
		{
			return;
		}
		Color color = TRAIL_BASE_COLOR;
		if (activate && newColor != null && !string.IsNullOrEmpty(newColor))
		{
			color = PandoraUtils.StringToColor(newColor);
		}
		for (int i = 0; i < trails.Count; i++)
		{
			WeaponTrail weaponTrail = trails[i];
			weaponTrail.Emit(activate);
			if (activate && weaponTrail.GetMaterial().HasProperty("_Color"))
			{
				weaponTrail.GetMaterial().SetColor("_Color", color);
			}
		}
	}

	public void EventShoot(int idx)
	{
		((RangeCombatFire)StateMachine.GetState(31)).ShootProjectile(idx);
	}

	public void EventSpellStart()
	{
		if (currentAction != null && currentAction.ActionId != UnitActionId.CHARGE && currentAction.ActionId != UnitActionId.AMBUSH && currentAction.fxData != null)
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(CurrentAction.fxData.RightHandFx, this, null, null);
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(CurrentAction.fxData.LeftHandFx, this, null, null);
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(CurrentAction.fxData.ChargeFx, (!CurrentAction.fxData.ChargeOnTarget || !(defenderCtrlr != null)) ? this : defenderCtrlr, null, null);
		}
	}

	public void EventSkillLaunch()
	{
		if (currentAction != null && currentAction.fxData != null)
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(CurrentAction.fxData.LaunchFx, this, null, null);
		}
	}

	public void EventSpellShoot()
	{
		if (IsCurrentState(State.SKILL_USE))
		{
			((SkillUse)StateMachine.GetState(30)).ShootProjectile();
		}
		else if (IsCurrentState(State.SPELL_CASTING))
		{
			((SpellCasting)StateMachine.GetState(28)).ShootProjectile();
		}
		else if (IsCurrentState(State.PERCEPTION))
		{
			((Perception)StateMachine.GetState(13)).LaunchFx();
		}
	}

	public void EventZoneAoe()
	{
		currentSpellTargetPosition = base.transform.position;
		ZoneAoe.Spawn(this, GetAction(SkillId.BASE_FLY));
	}

	public void EventAttachProj(int variation)
	{
		if (variation == 0)
		{
			if (base.Equipments[(int)unit.ActiveWeaponSlot] != null)
			{
				GameObject projectile = base.Equipments[(int)unit.ActiveWeaponSlot].GetProjectile();
				MeshFilter componentInChildren = projectile.GetComponentInChildren<MeshFilter>();
				projectile.transform.SetParent(base.BonesTr[BoneId.RIG_WEAPONR]);
				Transform transform = projectile.transform;
				float[] array = new float[3];
				Vector3 extents = componentInChildren.mesh.bounds.extents;
				array[0] = extents.x;
				Vector3 extents2 = componentInChildren.mesh.bounds.extents;
				array[1] = extents2.y;
				Vector3 extents3 = componentInChildren.mesh.bounds.extents;
				array[2] = extents3.z;
				transform.localPosition = new Vector3(0f, 0f, Mathf.Max(array) * 2f);
				projectile.transform.localRotation = Quaternion.identity;
			}
		}
		else if (base.Equipments[(int)unit.ActiveWeaponSlot] != null)
		{
			base.Equipments[(int)unit.ActiveWeaponSlot].AttachProjectile();
		}
	}

	public void EventReloadWeapons(int slot)
	{
		Reload reload = (Reload)StateMachine.GetState(19);
		reload.ReloadWeapon(slot);
	}

	public void EventWeaponAim()
	{
		RangeCombatFire rangeCombatFire = (RangeCombatFire)StateMachine.GetState(31);
		rangeCombatFire.WeaponAim();
	}

	public void EventInteract()
	{
		if (IsCurrentState(State.SKILL_USE))
		{
			((SkillUse)StateMachine.GetState(30)).OnInteract();
		}
		if (IsCurrentState(State.ACTIVATE))
		{
			((Activate)StateMachine.GetState(16)).ActivatePoint();
		}
		else if ((IsCurrentState(State.SEARCH) || IsCurrentState(State.INVENTORY)) && interactivePoint != null && interactivePoint is SearchPoint)
		{
			((SearchPoint)interactivePoint).Open();
		}
	}

	public override void EventDissolve()
	{
		Imprint.alwaysHide = true;
		Hide(hide: true);
	}

	public override void Hide(bool hide, bool force = false, UnityAction onDissolved = null)
	{
		base.Hide(hide || Imprint.alwaysHide, force, onDissolved);
	}

	public void EventFly()
	{
		if (IsCurrentState(State.FLY))
		{
			((Fly)StateMachine.GetState(48)).FlyToPoint();
		}
	}

	public int GetCRC()
	{
		int num = 0;
		for (int i = 0; i < base.Equipments.Count; i++)
		{
			if (base.Equipments[i] != null && base.Equipments[i].Item != null && base.Equipments[i].Item.Save != null)
			{
				num += base.Equipments[i].Item.Save.GetCRC(read: false);
			}
		}
		return num + unit.GetCRC();
	}

	public void RegisterToHermes()
	{
		PandoraSingleton<Hermes>.Instance.RegisterMyrtilus(this, needUID: false);
	}

	public void RemoveFromHermes()
	{
		PandoraSingleton<Hermes>.Instance.RemoveMyrtilus(this);
	}

	public void Send(bool reliable, Hermes.SendTarget target, uint id, uint command, params object[] parms)
	{
		if (command != 1)
		{
			SendSpeedPosition();
			if (commandSent)
			{
				Command item = default(Command);
				item.reliable = reliable;
				item.target = target;
				item.from = uid;
				item.command = command;
				item.parms = parms;
				commandsToSend.Enqueue(item);
				return;
			}
			commandSent = true;
		}
		PandoraSingleton<Hermes>.Instance.Send(reliable, target, id, command, GetCRC(), parms);
	}

	public void Receive(ulong from, uint command, object[] parms)
	{
		Command command2 = default(Command);
		command2.from = from;
		command2.command = command;
		if (parms != null)
		{
			command2.parms = (object[])parms.Clone();
		}
		switch (command)
		{
		case 1u:
			RunCommand(command2);
			break;
		case 17u:
			if (CanLaunchCommand() && StateMachine.GetActiveStateId() != 38)
			{
				RunCommand(command2);
			}
			break;
		default:
		{
			int cRC = GetCRC();
			commands.Enqueue(command2);
			PandoraDebug.LogInfo("Queuing Command = " + (CommandList)command + " State = " + (State)StateMachine.GetActiveStateId(), "MYRTILUS", this);
			break;
		}
		}
	}

	private void RunCommand(Command com)
	{
		uint command = com.command;
		object[] parms = com.parms;
		if (command != 1)
		{
			PandoraDebug.LogInfo("Running Command " + (CommandList)command + " at pos = " + base.transform.position + " at rot = " + base.transform.rotation + " at state = " + (State)StateMachine.GetActiveStateId(), "MYRTILUS", this);
		}
		object[] array = (object[])parms[1];
		switch (command)
		{
		case 1u:
		{
			float speed = (float)array[0];
			Quaternion rot2 = (Quaternion)array[1];
			Vector3 pos2 = (Vector3)array[2];
			NetworkSyncRPC(speed, rot2, pos2);
			break;
		}
		case 2u:
		{
			int skillId5 = (int)array[0];
			SkillRPC(skillId5);
			break;
		}
		case 3u:
		{
			int skillId4 = (int)array[0];
			uint targetUID3 = (uint)array[1];
			SkillSingleTargetRPC(skillId4, targetUID3);
			break;
		}
		case 23u:
		{
			int skillId3 = (int)array[0];
			uint destructUID = (uint)array[1];
			SkillSingleDestructibleRPC(skillId3, destructUID);
			break;
		}
		case 4u:
		{
			int skillId2 = (int)array[0];
			Vector3 targetPos = (Vector3)array[1];
			Vector3 targetDir = (Vector3)array[2];
			SkillTargetsRPC(skillId2, targetPos, targetDir);
			break;
		}
		case 5u:
		{
			uint senderUID2 = (uint)array[0];
			Vector3 currentUnitPos5 = (Vector3)array[1];
			Quaternion currentUnitRot5 = (Quaternion)array[2];
			OverwatchRPC(senderUID2, currentUnitPos5, currentUnitRot5);
			break;
		}
		case 6u:
		{
			uint targetUID2 = (uint)array[0];
			Vector3 currentUnitPos4 = (Vector3)array[1];
			Quaternion currentUnitRot4 = (Quaternion)array[2];
			AmbushRPC(targetUID2, currentUnitPos4, currentUnitRot4);
			break;
		}
		case 7u:
		{
			int itemIndex = (int)array[0];
			int slotIndex = (int)array[1];
			InventoryChangeRPC(itemIndex, slotIndex);
			break;
		}
		case 8u:
			InventoryDoneRPC();
			break;
		case 9u:
		{
			int skillId = (int)array[0];
			int actionZoneIdx = (int)array[1];
			InteractiveRPC(skillId, actionZoneIdx);
			break;
		}
		case 10u:
			ActionDoneRPC();
			break;
		case 11u:
		{
			Vector3 currentUnitPos3 = (Vector3)array[0];
			Quaternion currentUnitRot3 = (Quaternion)array[1];
			bool charge = (bool)array[2];
			EngagedRPC(currentUnitPos3, currentUnitRot3, charge);
			break;
		}
		case 12u:
		{
			Vector3 currentUnitPos2 = (Vector3)array[0];
			Quaternion currentUnitRot2 = (Quaternion)array[1];
			StartMoveRPC(currentUnitPos2, currentUnitRot2);
			break;
		}
		case 13u:
		{
			Vector3 unitPos = (Vector3)array[0];
			Quaternion unitRot = (Quaternion)array[1];
			int triggerIdx = (int)array[2];
			TriggerActivatedRPC(unitPos, unitRot, triggerIdx);
			break;
		}
		case 14u:
			InventoryTakeAllRPC();
			break;
		case 15u:
		{
			int zoneAoeIdx = (int)array[0];
			bool entering = (bool)array[1];
			ZoneAoeCrossRPC(zoneAoeIdx, entering);
			break;
		}
		case 16u:
			CurseRPC();
			break;
		case 17u:
		{
			UnitActionId actionId = (UnitActionId)(int)array[0];
			uint senderUID = (uint)array[1];
			Vector3 currentUnitPos = (Vector3)array[2];
			Quaternion currentUnitRot = (Quaternion)array[3];
			AskInterruption(actionId, senderUID, currentUnitPos, currentUnitRot);
			break;
		}
		case 18u:
		{
			uint targetUID = (uint)array[0];
			Vector3 pos = (Vector3)array[1];
			Quaternion rot = (Quaternion)array[2];
			MoveAndUpdateCircleRPC(targetUID, pos, rot);
			break;
		}
		case 19u:
			AthleticRPC();
			break;
		case 20u:
		{
			bool success = (bool)array[0];
			int action = (int)array[1];
			AthleticFinishedRPC(success, action);
			break;
		}
		case 21u:
			FlyRPC();
			break;
		case 22u:
		{
			uint[] engagedUids = (uint[])array[0];
			bool applyEnchants = (bool)array[1];
			bool previouslyEngaged = (bool)array[2];
			NewEngagedUnitsRPC(engagedUids, applyEnchants, previouslyEngaged);
			break;
		}
		}
	}
}
