using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Unit
{
	public static readonly AttributeId[] PhysicalAttributeIds;

	public static readonly AttributeId[] MentalAttributeIds;

	public static readonly AttributeId[] MartialAttributeIds;

	public static readonly InjuryId[] HIRE_UNIT_INJURY_EXCLUDES;

	public int warbandIdx;

	public int warbandPos;

	public bool isAI;

	private UnitSlotId activeWeaponSlot;

	public AnimStyleId currentAnimStyleId;

	private List<Item> items;

	private List<BodyPartData> availableBodyParts;

	public Dictionary<BodyPartId, BodyPart> bodyParts;

	public Dictionary<AttributeId, List<AttributeMod>> attributeModifiers;

	public Item deathTrophy;

	public int tempStrategyPoints;

	public int tempOffensePoints;

	private int[] attributes;

	private int[] tempAttributes;

	private static AttributeData[] attributeDataById;

	private static Dictionary<AttributeId, List<AttributeAttributeData>> attributeAttributeDataById;

	private Dictionary<int, AttributeId> maxAttributes;

	private Dictionary<UnitActionId, CostModifier> actionCostModifiers;

	private Dictionary<SkillId, CostModifier> skillCostModifiers;

	private Dictionary<SkillId, int> damagePercModifiers;

	private Dictionary<SpellTypeId, CostModifier> spellTypeModifiers;

	private Dictionary<UnitActionId, EnchantmentId> blockedActions;

	private Dictionary<SkillId, EnchantmentId> blockedSkills;

	private Dictionary<BoneId, EnchantmentId> blockedBones;

	private Dictionary<ItemTypeId, EnchantmentId> blockedItemTypes;

	private UnitStateId newState;

	private List<AttributeData> attributeDataList;

	private List<EnchantmentTypeId> enchantTypeImmunities;

	private List<EnchantmentTypeId> enchantTypeToBeRemoved;

	private List<EnchantmentId> enchantToBeRemoved;

	private List<UnitJoinAttributeData> baseAttributesData;

	private List<CampaignUnitJoinAttributeData> campaignModifiers;

	private List<UnitTypeAttributeData> baseAttributesDataMax;

	private List<UnitJoinUnitRankData> ranksData;

	private int totalPhysicalPoints;

	private int spentPhysicalPoints;

	private int totalMentalPoints;

	private int spentMentalPoints;

	private int totalMartialPoints;

	private int spentMartialPoints;

	private int totalSkillPoints;

	private int spentSkillPoints;

	private int totalSpellPoints;

	private int spentSpellPoints;

	private AttributeModList weaponDamageModifiers = new AttributeModList();

	private bool needFxRefresh;

	private int realBackPackCapacity;

	private int cachedBackpackCapacity = -1;

	private Unit stunningUnit;

	private int baseMoralImpact;

	private readonly Dictionary<InjuryId, int> injuryRollModifiers = new Dictionary<InjuryId, int>(InjuryIdComparer.Instance);

	public string Name => UnitSave.stats.Name;

	public UnitId Id => Data.Id;

	public RaceId RaceId => Data.RaceId;

	public WarbandId WarbandId => Data.WarbandId;

	public AllegianceId AllegianceId => (WarData.AllegianceId == AllegianceId.NONE) ? Data.AllegianceId : WarData.AllegianceId;

	public UnitData Data
	{
		get;
		private set;
	}

	public UnitBaseData BaseData
	{
		get;
		private set;
	}

	public UnitSave UnitSave
	{
		get;
		private set;
	}

	public EventLogger Logger
	{
		get;
		private set;
	}

	public CampaignUnitData CampaignData
	{
		get;
		private set;
	}

	public bool NoLootBag => CampaignData != null && CampaignData.NoLootBag;

	public WarbandData WarData
	{
		get;
		private set;
	}

	public bool Active => GetActiveStatus() == UnitActiveStatusId.AVAILABLE && UnitSave.warbandSlotIndex < 12;

	public UnitStateId Status
	{
		get;
		set;
	}

	public UnitStateId PreviousStatus
	{
		get;
		set;
	}

	public string LocalizedType
	{
		get;
		set;
	}

	public string LocalizedName
	{
		get;
		set;
	}

	public string LocalizedDescription
	{
		get;
		set;
	}

	public bool IsLeader => GetUnitTypeId() == UnitTypeId.LEADER;

	public bool IsActiveLeader => IsLeader && UnitSave.warbandSlotIndex < 12;

	public bool IsImpressive => GetUnitTypeId() == UnitTypeId.IMPRESSIVE;

	public bool IsMonster => GetUnitTypeId() == UnitTypeId.MONSTER || Id == UnitId.CHAOS_OGRE;

	public bool IsSpellcaster => UnitSave.spells.Count > 0;

	public int CurrentWound
	{
		get
		{
			return GetAttribute(AttributeId.CURRENT_WOUND);
		}
		set
		{
			SetAttribute(AttributeId.CURRENT_WOUND, Mathf.Clamp(value, 0, GetAttribute(AttributeId.WOUND)));
		}
	}

	public UnitSlotId ActiveWeaponSlot
	{
		get
		{
			return activeWeaponSlot;
		}
		set
		{
			activeWeaponSlot = value;
			UpdateAttributes();
		}
	}

	public UnitSlotId InactiveWeaponSlot => (ActiveWeaponSlot != UnitSlotId.SET1_MAINHAND) ? UnitSlotId.SET1_MAINHAND : UnitSlotId.SET2_MAINHAND;

	public List<Enchantment> Enchantments
	{
		get;
		private set;
	}

	public List<Item> Items
	{
		get
		{
			int num = 6 + BackpackCapacity;
			if (num != items.Count)
			{
				for (int i = items.Count; i < num; i++)
				{
					items.Add(new Item(ItemId.NONE));
				}
				for (int num2 = items.Count - 1; num2 >= num; num2--)
				{
					items.RemoveAt(num2);
				}
			}
			return items;
		}
		private set
		{
			items = value;
		}
	}

	public List<Item> ActiveItems
	{
		get;
		private set;
	}

	public List<Mutation> Mutations
	{
		get;
		private set;
	}

	public List<Injury> Injuries
	{
		get;
		private set;
	}

	public List<SkillData> ActiveSkills
	{
		get;
		private set;
	}

	public List<SkillData> PassiveSkills
	{
		get;
		private set;
	}

	public List<SkillData> ConsumableSkills
	{
		get;
		private set;
	}

	public List<SkillData> Spells
	{
		get;
		private set;
	}

	public SkillData ActiveSkill
	{
		get;
		private set;
	}

	public bool UnitAlwaysVisible
	{
		get;
		set;
	}

	public int UnspentPhysical
	{
		get
		{
			int num = 0;
			for (int i = 0; i < PhysicalAttributeIds.Length; i++)
			{
				num += GetTempAttribute(PhysicalAttributeIds[i]);
			}
			return totalPhysicalPoints - spentPhysicalPoints - num;
		}
	}

	public int UnspentMental
	{
		get
		{
			int num = 0;
			for (int i = 0; i < MentalAttributeIds.Length; i++)
			{
				num += GetTempAttribute(MentalAttributeIds[i]);
			}
			return totalMentalPoints - spentMentalPoints - num;
		}
	}

	public int UnspentMartial
	{
		get
		{
			int num = 0;
			for (int i = 0; i < MartialAttributeIds.Length; i++)
			{
				num += GetTempAttribute(MartialAttributeIds[i]);
			}
			return totalMartialPoints - spentMartialPoints - num;
		}
	}

	public int UnspentSkill => totalSkillPoints - spentSkillPoints;

	public int UnspentSpell => totalSpellPoints - spentSpellPoints;

	public int BackpackCapacity => (cachedBackpackCapacity == -1) ? realBackPackCapacity : cachedBackpackCapacity;

	public bool HasEnchantmentsChanged
	{
		get;
		private set;
	}

	public int Movement => GetAttribute(AttributeId.MOVEMENT);

	public int WeaponSkill => GetAttribute(AttributeId.WEAPON_SKILL);

	public int BallisticSkill => GetAttribute(AttributeId.BALLISTIC_SKILL);

	public int Strength => GetAttribute(AttributeId.STRENGTH);

	public int Toughness => GetAttribute(AttributeId.TOUGHNESS);

	public int Wound => GetAttribute(AttributeId.WOUND);

	public int Agility => GetAttribute(AttributeId.AGILITY);

	public int Leadership => GetAttribute(AttributeId.LEADERSHIP);

	public int Moral => GetAttribute(AttributeId.MORAL);

	public int MoralImpact => Mathf.Max(0, GetAttribute(AttributeId.MORAL_IMPACT));

	public int Alertness => GetAttribute(AttributeId.ALERTNESS);

	public int Accuracy => GetAttribute(AttributeId.ACCURACY);

	public int Intelligence => GetAttribute(AttributeId.INTELLIGENCE);

	public int Initiative => (Status != UnitStateId.OUT_OF_ACTION) ? GetAttribute(AttributeId.INITIATIVE) : 0;

	public int CurrentStrategyPoints => GetAttribute(AttributeId.CURRENT_STRATEGY_POINTS);

	public int StrategyPoints => GetAttribute(AttributeId.STRATEGY_POINTS);

	public int CurrentOffensePoints => GetAttribute(AttributeId.CURRENT_OFFENSE_POINTS);

	public int OffensePoints => GetAttribute(AttributeId.OFFENSE_POINTS);

	public int WoundPerc => GetAttribute(AttributeId.WOUND_PERC);

	public int ArmorAbsorption => GetAttribute(AttributeId.ARMOR_ABSORPTION);

	public int BypassArmor => GetAttribute(AttributeId.BYPASS_ARMOR);

	public int ArmorAbsorptionPerc => GetAttribute(AttributeId.ARMOR_ABSORPTION_PERC);

	public int ByPassArmorPerc => GetAttribute(AttributeId.BYPASS_ARMOR_PERC);

	public int DamageBonus => GetAttribute(AttributeId.DAMAGE_BONUS);

	public int DamageBonusMelee => GetAttribute(AttributeId.DAMAGE_BONUS_MELEE);

	public int DamageBonusRange => GetAttribute(AttributeId.DAMAGE_BONUS_RANGE);

	public int DamageBonusSpell => GetAttribute(AttributeId.DAMAGE_BONUS_SPELL);

	public int DamageBonusMeleePerc => GetAttribute(AttributeId.DAMAGE_BONUS_MELEE_PERC);

	public int DamageBonusRangePerc => GetAttribute(AttributeId.DAMAGE_BONUS_RANGE_PERC);

	public int DamageBonusDivMagPerc => GetAttribute(AttributeId.DAMAGE_BONUS_DIV_MAG_PERC);

	public int DamageBonusArcMagPerc => GetAttribute(AttributeId.DAMAGE_BONUS_ARC_MAG_PERC);

	public int DamageBonusChargePerc => GetAttribute(AttributeId.DAMAGE_BONUS_CHARGE_PERC);

	public int DamageMin => GetAttribute(AttributeId.DAMAGE_MIN);

	public int DamageMax => GetAttribute(AttributeId.DAMAGE_MAX);

	public int DamageCriticalBonus => GetAttribute(AttributeId.DAMAGE_CRITICAL_BONUS);

	public int DamageCriticalBonusPerc => GetAttribute(AttributeId.DAMAGE_CRITICAL_BONUS_PERC);

	public int DamageHoly => GetAttribute(AttributeId.DAMAGE_HOLY);

	public int DamageUnholy => GetAttribute(AttributeId.DAMAGE_UNHOLY);

	public int DamageBonusHolyPerc => GetAttribute(AttributeId.DAMAGE_BONUS_HOLY_PERC);

	public int DamageBonusUnholyPerc => GetAttribute(AttributeId.DAMAGE_BONUS_UNHOLY_PERC);

	public int GlobalRangeDamagePerc => GetAttribute(AttributeId.GLOBAL_RANGE_DAMAGE_PERC);

	public int GlobalMeleeDamagePerc => GetAttribute(AttributeId.GLOBAL_MELEE_DAMAGE_PERC);

	public int CritResistance => GetAttribute(AttributeId.CRIT_RESISTANCE);

	public int MagicResistance => GetAttribute(AttributeId.MAGIC_RESISTANCE);

	public int MagicResistDefenderModifier => GetAttribute(AttributeId.MAGIC_RESIST_DEFENDER_MODIFIER);

	public int DodgeDefenderModifier => GetAttribute(AttributeId.DODGE_DEFENDER_MODIFIER);

	public int ParryDefenderModifier => GetAttribute(AttributeId.PARRY_DEFENDER_MODIFIER);

	public int MeleeResistance => GetAttribute(AttributeId.MELEE_RESISTANCE);

	public int RangeResistance => GetAttribute(AttributeId.RANGE_RESISTANCE);

	public int RangeResistanceDefenderModifier => GetAttribute(AttributeId.RANGE_RESISTANCE_DEFENDER_MODIFIER);

	public int PoisonResistDefenderModifier => GetAttribute(AttributeId.POISON_RESIST_DEFENDER_MODIFIER);

	public int Xp => UnitSave.xp;

	public int Rank => GetAttribute(AttributeId.RANK);

	public int ViewDistance => GetAttribute((!isAI) ? AttributeId.VIEW_DISTANCE : AttributeId.AI_VIEW_DISTANCE);

	public int RangeBonusSpell => GetAttribute(AttributeId.RANGE_BONUS_SPELL);

	public int ParryLeft => GetAttribute(AttributeId.PARRY_LEFT);

	public int DodgeLeft => GetAttribute(AttributeId.DODGE_LEFT);

	public int AmbushLeft => GetAttribute(AttributeId.AMBUSH_LEFT);

	public int OverwatchLeft => GetAttribute(AttributeId.OVERWATCH_LEFT);

	public int AttackPerAction => GetAttribute(AttributeId.ATTACK_PER_ACTION);

	public int MaxAgility => GetAttribute(AttributeId.AGILITY_MAX);

	public int MaxStrength => GetAttribute(AttributeId.STRENGTH_MAX);

	public int MaxToughness => GetAttribute(AttributeId.TOUGHNESS_MAX);

	public int MaxLeadership => GetAttribute(AttributeId.LEADERSHIP_MAX);

	public int MaxIntelligence => GetAttribute(AttributeId.INTELLIGENCE_MAX);

	public int MaxAlertness => GetAttribute(AttributeId.ALERTNESS_MAX);

	public int MaxWeaponSkill => GetAttribute(AttributeId.WEAPON_SKILL_MAX);

	public int MaxBallisticSkill => GetAttribute(AttributeId.BALLISTIC_SKILL_MAX);

	public int MaxAccuracy => GetAttribute(AttributeId.ACCURACY_MAX);

	public int CounterDisabled => GetAttribute(AttributeId.COUNTER_DISABLED);

	public int CounterForced => GetAttribute(AttributeId.COUNTER_FORCED);

	public int ChargeMovement => GetAttribute(AttributeId.CHARGE_MOVEMENT);

	public int AmbushMovement => GetAttribute(AttributeId.AMBUSH_MOVEMENT);

	public int DodgeBypass => GetAttribute(AttributeId.DODGE_BYPASS);

	public int ParryBypass => GetAttribute(AttributeId.PARRY_BYPASS);

	public int ClimbRoll3 => GetAttribute(AttributeId.CLIMB_ROLL_3);

	public int ClimbRoll6 => GetAttribute(AttributeId.CLIMB_ROLL_6);

	public int ClimbRoll9 => GetAttribute(AttributeId.CLIMB_ROLL_9);

	public int LeapRoll => GetAttribute(AttributeId.LEAP_ROLL);

	public int JumpDownRoll3 => GetAttribute(AttributeId.JUMP_DOWN_ROLL_3);

	public int JumpDownRoll6 => GetAttribute(AttributeId.JUMP_DOWN_ROLL_6);

	public int JumpDownRoll9 => GetAttribute(AttributeId.JUMP_DOWN_ROLL_9);

	public int LockpickingRoll => GetAttribute(AttributeId.LOCKPICKING_ROLL);

	public int PoisonResistRoll => GetAttribute(AttributeId.POISON_RESIST_ROLL);

	public int TrapResistRoll => GetAttribute(AttributeId.TRAP_RESIST_ROLL);

	public int LeadershipRoll => GetAttribute(AttributeId.LEADERSHIP_ROLL);

	public int AllAloneRoll => GetAttribute(AttributeId.ALL_ALONE_ROLL);

	public int FearRoll => GetAttribute(AttributeId.FEAR_ROLL);

	public int TerrorRoll => GetAttribute(AttributeId.TERROR_ROLL);

	public int WarbandRoutRoll => GetAttribute(AttributeId.WARBAND_ROUT_ROLL);

	public int SpellcastingRoll => GetAttribute(AttributeId.SPELLCASTING_ROLL);

	public int DivineSpellcastingRoll => GetAttribute(AttributeId.DIVINE_SPELLCASTING_ROLL);

	public int ArcaneSpellcastingRoll => GetAttribute(AttributeId.ARCANE_SPELLCASTING_ROLL);

	public int TzeentchsCurseRoll => GetAttribute(AttributeId.TZEENTCHS_CURSE_ROLL);

	public int DivineWrathRoll => GetAttribute(AttributeId.DIVINE_WRATH_ROLL);

	public int PerceptionRoll => GetAttribute(AttributeId.PERCEPTION_ROLL);

	public int CombatMeleeHitRoll => GetAttribute(AttributeId.COMBAT_MELEE_HIT_ROLL);

	public int CombatRangeHitRoll => GetAttribute(AttributeId.COMBAT_RANGE_HIT_ROLL);

	public int CriticalMeleeAttemptRoll => GetAttribute(AttributeId.CRITICAL_MELEE_ATTEMPT_ROLL);

	public int CriticalRangeAttemptRoll => GetAttribute(AttributeId.CRITICAL_RANGE_ATTEMPT_ROLL);

	public int CriticalResultRoll => GetAttribute(AttributeId.CRITICAL_RESULT_ROLL);

	public int StupidityRoll => GetAttribute(AttributeId.STUPIDITY_ROLL);

	public int ParryingRoll => GetAttribute(AttributeId.PARRYING_ROLL);

	public int DodgeRoll => GetAttribute(AttributeId.DODGE_ROLL);

	public int InjuryRoll => GetAttribute(AttributeId.INJURY_ROLL);

	public int WyrdstoneResistRoll => GetAttribute(AttributeId.WYRDSTONE_RESIST_ROLL);

	public int StunResistRoll => GetAttribute(AttributeId.STUN_RESIST_ROLL);

	public Unit(UnitSave us)
	{
		UnitSave = us;
		Init();
	}

	static Unit()
	{
		PhysicalAttributeIds = new AttributeId[3]
		{
			AttributeId.STRENGTH,
			AttributeId.TOUGHNESS,
			AttributeId.AGILITY
		};
		MentalAttributeIds = new AttributeId[3]
		{
			AttributeId.LEADERSHIP,
			AttributeId.INTELLIGENCE,
			AttributeId.ALERTNESS
		};
		MartialAttributeIds = new AttributeId[3]
		{
			AttributeId.WEAPON_SKILL,
			AttributeId.BALLISTIC_SKILL,
			AttributeId.ACCURACY
		};
		HIRE_UNIT_INJURY_EXCLUDES = new InjuryId[5]
		{
			InjuryId.DEAD,
			InjuryId.MULTIPLE_INJURIES,
			InjuryId.NEAR_DEATH,
			InjuryId.FULL_RECOVERY,
			InjuryId.AMNESIA
		};
	}

	public static Unit GenerateUnit(UnitId unitId, int rank)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Unit unit = new Unit(new UnitSave(unitId));
		PandoraDebug.LogDebug("iNIT uNIT" + (Time.realtimeSinceStartup - realtimeSinceStartup) + "s", "LOADING");
		if (rank > 0)
		{
			List<UnitJoinUnitRankData> advancements = new List<UnitJoinUnitRankData>();
			List<Mutation> newMutations = new List<Mutation>();
			List<Item> previousItems = new List<Item>();
			unit.AddXp(99999999, advancements, newMutations, previousItems, 0, rank);
			unit.UnitSave.xp = 0;
			unit.UnitSave.items.Clear();
			for (int i = 0; i < 13; i++)
			{
				unit.UnitSave.items.Add(null);
			}
			unit.InitItems();
		}
		PandoraDebug.LogDebug("iNIT uNIT gENERATE uNIT fINISHED" + (Time.realtimeSinceStartup - realtimeSinceStartup) + "s", "LOADING");
		return unit;
	}

	public static Unit GenerateHireUnit(UnitId unitId, int warbandRank, int unitRank)
	{
		Unit unit = GenerateUnit(unitId, unitRank);
		List<Item> removedItems = new List<Item>();
		List<InjuryId> list = new List<InjuryId>(HIRE_UNIT_INJURY_EXCLUDES);
		HireUnitInjuryData randomRatio = HireUnitInjuryData.GetRandomRatio(PandoraSingleton<DataFactory>.Instance.InitData<HireUnitInjuryData>("unit_rank", unit.Rank.ToConstantString()), PandoraSingleton<GameManager>.Instance.LocalTyche);
		for (int i = 0; i < randomRatio.Count; i++)
		{
			InjuryData injuryData = PandoraSingleton<HideoutManager>.Instance.Progressor.RollInjury(list, unit);
			if (injuryData != null)
			{
				unit.AddInjury(injuryData, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, removedItems, isHireUnit: true);
				list.Add(injuryData.Id);
				continue;
			}
			break;
		}
		List<HireUnitItemData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<HireUnitItemData>("unit_rank", unit.Rank.ToConstantString());
		List<HireUnitItemQualityData> datas = PandoraSingleton<DataFactory>.Instance.InitData<HireUnitItemQualityData>("fk_warband_rank_id", warbandRank.ToConstantString());
		HireUnitItemRunemarkData hireUnitItemRunemarkData = PandoraSingleton<DataFactory>.Instance.InitData<HireUnitItemRunemarkData>("fk_warband_rank_id", warbandRank.ToConstantString())[0];
		int ratingPool = 0;
		List<Item> list3 = new List<Item>();
		CombatStyleId excludedCombatStyleId = CombatStyleId.NONE;
		bool flag = false;
		bool flag2 = false;
		for (int j = 0; j < list2.Count; j++)
		{
			if (PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, 100) >= list2[j].Ratio)
			{
				continue;
			}
			switch (list2[j].UnitSlotId)
			{
			case UnitSlotId.HELMET:
				flag = true;
				break;
			case UnitSlotId.ARMOR:
				flag2 = true;
				break;
			case UnitSlotId.SET1_MAINHAND:
			case UnitSlotId.SET2_MAINHAND:
				excludedCombatStyleId = UnitFactory.AddCombatStyleSet(PandoraSingleton<GameManager>.Instance.LocalTyche, ref ratingPool, unit, list2[j].UnitSlotId, excludedCombatStyleId, HireUnitItemQualityData.GetRandomRatio(datas, PandoraSingleton<GameManager>.Instance.LocalTyche).ItemQualityId, PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, 100) < hireUnitItemRunemarkData.Ratio, list3);
				break;
			case UnitSlotId.ITEM_1:
			case UnitSlotId.ITEM_2:
			case UnitSlotId.ITEM_3:
			case UnitSlotId.ITEM_4:
			case UnitSlotId.ITEM_5:
			case UnitSlotId.ITEM_6:
			case UnitSlotId.ITEM_7:
				if (!unit.BothArmsMutated() && (int)list2[j].UnitSlotId < unit.Items.Count)
				{
					Item procItem = UnitFactory.GetProcItem(PandoraSingleton<GameManager>.Instance.LocalTyche, ref ratingPool, unit, UnitSlotId.ITEM_1, ItemTypeId.CONSUMABLE_POTIONS, HireUnitItemQualityData.GetRandomRatio(datas, PandoraSingleton<GameManager>.Instance.LocalTyche).ItemQualityId);
					list3.Add(procItem);
					unit.EquipItem(list2[j].UnitSlotId, procItem);
				}
				break;
			}
		}
		if (flag2 || flag)
		{
			UnitFactory.AddArmorStyleSet(PandoraSingleton<GameManager>.Instance.LocalTyche, ref ratingPool, unit, HireUnitItemQualityData.GetRandomRatio(datas, PandoraSingleton<GameManager>.Instance.LocalTyche).ItemQualityId, flag2, flag, flag2 && PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, 100) < hireUnitItemRunemarkData.Ratio, flag && PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, 100) < hireUnitItemRunemarkData.Ratio, list3);
		}
		unit.ResetBodyPart();
		return unit;
	}

	private void Init()
	{
		Enchantments = new List<Enchantment>();
		enchantTypeImmunities = new List<EnchantmentTypeId>();
		enchantTypeToBeRemoved = new List<EnchantmentTypeId>();
		enchantToBeRemoved = new List<EnchantmentId>();
		ActiveSkills = new List<SkillData>();
		PassiveSkills = new List<SkillData>();
		ConsumableSkills = new List<SkillData>();
		Spells = new List<SkillData>();
		Mutations = new List<Mutation>();
		Injuries = new List<Injury>();
		actionCostModifiers = new Dictionary<UnitActionId, CostModifier>(UnitActionIdComparer.Instance);
		skillCostModifiers = new Dictionary<SkillId, CostModifier>(SkillIdComparer.Instance);
		damagePercModifiers = new Dictionary<SkillId, int>(SkillIdComparer.Instance);
		spellTypeModifiers = new Dictionary<SpellTypeId, CostModifier>(SpellTypeIdComparer.Instance);
		blockedActions = new Dictionary<UnitActionId, EnchantmentId>(UnitActionIdComparer.Instance);
		blockedSkills = new Dictionary<SkillId, EnchantmentId>(SkillIdComparer.Instance);
		blockedBones = new Dictionary<BoneId, EnchantmentId>(BoneIdComparer.Instance);
		blockedItemTypes = new Dictionary<ItemTypeId, EnchantmentId>(ItemTypeIdComparer.Instance);
		attributes = new int[152];
		availableBodyParts = new List<BodyPartData>();
		bodyParts = new Dictionary<BodyPartId, BodyPart>(BodyPartIdComparer.Instance);
		tempAttributes = new int[152];
		maxAttributes = new Dictionary<int, AttributeId>();
		ActiveItems = new List<Item>();
		Logger = new EventLogger(UnitSave.stats.history);
		Data = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(UnitSave.stats.id);
		BaseData = PandoraSingleton<DataFactory>.Instance.InitData<UnitBaseData>((int)Data.UnitBaseId);
		WarData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)Data.WarbandId);
		UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)UnitSave.rankId);
		SetAttribute(AttributeId.RANK, unitRankData.Rank);
		SetMoralImpact();
		attributeDataList = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>();
		if (attributeDataById == null)
		{
			attributeDataById = new AttributeData[152];
			for (int i = 0; i < attributeDataList.Count; i++)
			{
				AttributeData attributeData = attributeDataList[i];
				attributeDataById[(int)attributeData.Id] = attributeData;
			}
			List<AttributeAttributeData> list = PandoraSingleton<DataFactory>.Instance.InitData<AttributeAttributeData>();
			attributeAttributeDataById = new Dictionary<AttributeId, List<AttributeAttributeData>>(AttributeIdComparer.Instance);
			for (int j = 0; j < list.Count; j++)
			{
				AttributeAttributeData attributeAttributeData = list[j];
				if (!attributeAttributeDataById.TryGetValue(attributeAttributeData.AttributeId, out List<AttributeAttributeData> value))
				{
					value = new List<AttributeAttributeData>();
					attributeAttributeDataById[attributeAttributeData.AttributeId] = value;
				}
				value.Add(attributeAttributeData);
			}
		}
		baseAttributesData = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinAttributeData>("fk_unit_id", ((int)Id).ToConstantString());
		ranksData = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinUnitRankData>("fk_unit_id", ((int)Id).ToConstantString());
		baseAttributesDataMax = PandoraSingleton<DataFactory>.Instance.InitData<UnitTypeAttributeData>("fk_unit_type_id", ((int)Data.UnitTypeId).ToConstantString());
		RefreshDescription();
		CalculateProgressionPoints();
		if (UnitSave.campaignId != 0)
		{
			CampaignData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignUnitData>(UnitSave.campaignId);
		}
		if (string.IsNullOrEmpty(UnitSave.stats.name))
		{
			UnitSave.stats.name = GetRandomName(Data.WarbandId, Data.Id);
		}
		if (string.IsNullOrEmpty(UnitSave.skinColor))
		{
			UnitSave.skinColor = Data.SkinColor;
		}
		List<UnitJoinEnchantmentData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinEnchantmentData>("fk_unit_id", Data.Id.ToIntString());
		for (int k = 0; k < list2.Count; k++)
		{
			Enchantments.Add(new Enchantment(list2[k].EnchantmentId, null, this, orig: true, innate: true));
		}
		for (int l = 0; l < ranksData.Count; l++)
		{
			if (ranksData[l].EnchantmentId != 0 && ranksData[l].UnitRankId <= unitRankData.Id)
			{
				Enchantments.Add(new Enchantment(ranksData[l].EnchantmentId, null, this, orig: true, innate: true));
			}
		}
		List<UnitJoinSkillData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkillData>("fk_unit_id", Data.Id.ToIntString());
		for (int m = 0; m < list3.Count; m++)
		{
			AddSkill(list3[m].SkillId, updateAttributes: false);
		}
		if (CampaignData != null)
		{
			List<CampaignUnitJoinEnchantmentData> list4 = PandoraSingleton<DataFactory>.Instance.InitData<CampaignUnitJoinEnchantmentData>("fk_campaign_unit_id", CampaignData.Id.ToIntString());
			for (int n = 0; n < list4.Count; n++)
			{
				Enchantments.Add(new Enchantment(list4[n].EnchantmentId, null, this, orig: true, innate: true));
			}
		}
		for (int num = 0; num < UnitSave.injuries.Count; num++)
		{
			Injuries.Add(new Injury(UnitSave.injuries[num], this));
		}
		for (int num2 = 0; num2 < UnitSave.mutations.Count; num2++)
		{
			Mutations.Add(new Mutation(UnitSave.mutations[num2], this));
		}
		for (int num3 = 0; num3 < UnitSave.activeSkills.Count; num3++)
		{
			AddSkill(UnitSave.activeSkills[num3], updateAttributes: false);
		}
		for (int num4 = 0; num4 < UnitSave.passiveSkills.Count; num4++)
		{
			AddSkill(UnitSave.passiveSkills[num4], updateAttributes: false);
		}
		for (int num5 = 0; num5 < UnitSave.consumableSkills.Count; num5++)
		{
			AddSkill(UnitSave.consumableSkills[num5], updateAttributes: false);
		}
		for (int num6 = 0; num6 < UnitSave.spells.Count; num6++)
		{
			AddSkill(UnitSave.spells[num6], updateAttributes: false);
		}
		for (int num7 = 0; num7 < ActiveSkills.Count; num7++)
		{
			RemovePrequesiteSkill(ActiveSkills[num7]);
		}
		for (int num8 = 0; num8 < PassiveSkills.Count; num8++)
		{
			RemovePrequesiteSkill(PassiveSkills[num8]);
		}
		for (int num9 = 0; num9 < Spells.Count; num9++)
		{
			RemovePrequesiteSkill(Spells[num9]);
		}
		attributeModifiers = new Dictionary<AttributeId, List<AttributeMod>>(AttributeIdComparer.Instance);
		List<AttributeData> list5 = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>();
		for (int num10 = 0; num10 < list5.Count; num10++)
		{
			int id = (int)list5[num10].Id;
			attributeModifiers[list5[num10].Id] = new List<AttributeMod>();
			if (list5[num10].Save)
			{
				if (!UnitSave.stats.stats.ContainsKey(id))
				{
					UnitSave.stats.stats[id] = 0;
				}
				SetAttribute(list5[num10].Id, UnitSave.stats.stats[id]);
			}
			if (list5[num10].AttributeIdMax != 0)
			{
				maxAttributes.Add(id, list5[num10].AttributeIdMax);
			}
		}
		if (CampaignData != null)
		{
			campaignModifiers = PandoraSingleton<DataFactory>.Instance.InitData<CampaignUnitJoinAttributeData>("fk_campaign_unit_id", ((int)CampaignData.Id).ToConstantString());
		}
		Items = new List<Item>();
		UpdateAttributes();
		foreach (BodyPart value3 in bodyParts.Values)
		{
			value3.DestroyRelatedGO();
		}
		List<BodyPartData> list6 = PandoraSingleton<DataFactory>.Instance.InitData<BodyPartData>();
		List<BodyPartUnitExcludedData> list7 = PandoraSingleton<DataFactory>.Instance.InitData<BodyPartUnitExcludedData>("fk_unit_id", Id.ToIntString());
		int num11 = 0;
		if (UnitSave.campaignId != 0)
		{
			CampaignWarbandJoinCampaignUnitData campaignWarbandJoinCampaignUnitData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandJoinCampaignUnitData>("fk_campaign_unit_id", UnitSave.campaignId.ToConstantString())[0];
			CampaignWarbandData campaignWarbandData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandData>((int)campaignWarbandJoinCampaignUnitData.CampaignWarbandId);
			num11 = (int)campaignWarbandData.ColorPresetId << 8;
		}
		else
		{
			num11 = (int)WarData.ColorPresetId << 8;
		}
		for (int num12 = 0; num12 < list6.Count; num12++)
		{
			BodyPartData bodyPartData = list6[num12];
			if (bodyPartData.Id == BodyPartId.NONE)
			{
				continue;
			}
			bool flag = string.IsNullOrEmpty(WarData.Asset);
			for (int num13 = 0; num13 < list7.Count; num13++)
			{
				if (flag)
				{
					break;
				}
				if (list7[num13].BodyPartId == bodyPartData.Id)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				availableBodyParts.Add(bodyPartData);
				if (!UnitSave.customParts.ContainsKey(bodyPartData.Id))
				{
					KeyValuePair<int, int> value2 = new KeyValuePair<int, int>(-1, num11);
					UnitSave.customParts[bodyPartData.Id] = value2;
				}
			}
		}
		InitItems();
		deathTrophy = new Item(Data.ItemIdTrophy);
		deathTrophy.owner = this;
		HasEnchantmentsChanged = false;
	}

	private void RefreshDescription()
	{
		LocalizedName = PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("unit_name_", Id.ToString());
		LocalizedType = string.Format("{0} / {1}", PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("unit_type_", GetUnitTypeId().ToString()), LocalizedName);
		LocalizedDescription = PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("unit_desc_", Id.ToString());
	}

	private void InitItems()
	{
		SetItems();
		ResetBodyPart();
		ActiveWeaponSlot = UnitSlotId.SET1_MAINHAND;
		SetAttribute(AttributeId.CURRENT_WOUND, Wound);
		Status = UnitStateId.NONE;
		PreviousStatus = UnitStateId.NONE;
		SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, 0);
		SetAttribute(AttributeId.CURRENT_OFFENSE_POINTS, 0);
		tempStrategyPoints = 0;
		tempOffensePoints = 0;
	}

	private void CalculateProgressionPoints()
	{
		totalPhysicalPoints = 0;
		totalMartialPoints = 0;
		totalMentalPoints = 0;
		totalSkillPoints = 0;
		totalSpellPoints = 0;
		spentPhysicalPoints = 0;
		spentMartialPoints = 0;
		spentMentalPoints = 0;
		spentSkillPoints = 0;
		spentSpellPoints = 0;
		for (int i = 0; i < ranksData.Count && ranksData[i].UnitRankId <= UnitSave.rankId; i++)
		{
			totalPhysicalPoints += ranksData[i].Physical;
			totalMartialPoints += ranksData[i].Martial;
			totalMentalPoints += ranksData[i].Mental;
			totalSkillPoints += ranksData[i].Skill;
			totalSpellPoints += ranksData[i].Spell;
		}
		for (int j = 0; j < UnitSave.passiveSkills.Count; j++)
		{
			AddSkillLearnBonusPoints(UnitSave.passiveSkills[j]);
			spentSkillPoints += SkillHelper.GetSkill(UnitSave.passiveSkills[j]).Points;
		}
		for (int k = 0; k < UnitSave.activeSkills.Count; k++)
		{
			spentSkillPoints += SkillHelper.GetSkill(UnitSave.activeSkills[k]).Points;
		}
		for (int l = 0; l < UnitSave.spells.Count; l++)
		{
			spentSpellPoints += SkillHelper.GetSkill(UnitSave.spells[l]).Points;
		}
		for (int m = 0; m < UnitSave.consumableSkills.Count; m++)
		{
			AddSkillLearnBonusPoints(UnitSave.consumableSkills[m]);
		}
		for (int n = 0; n < PhysicalAttributeIds.Length; n++)
		{
			spentPhysicalPoints += GetSaveAttribute(PhysicalAttributeIds[n]);
		}
		for (int num = 0; num < MentalAttributeIds.Length; num++)
		{
			spentMentalPoints += GetSaveAttribute(MentalAttributeIds[num]);
		}
		for (int num2 = 0; num2 < MartialAttributeIds.Length; num2++)
		{
			spentMartialPoints += GetSaveAttribute(MartialAttributeIds[num2]);
		}
		if (UnitSave.skillInTrainingId != 0)
		{
			SkillData skill = SkillHelper.GetSkill(UnitSave.skillInTrainingId);
			if (skill.SkillTypeId == SkillTypeId.SPELL_ACTION)
			{
				spentSpellPoints += skill.Points;
			}
			else
			{
				spentSkillPoints += skill.Points;
			}
		}
	}

	private void AddSkillLearnBonusPoints(SkillId skillId)
	{
		List<SkillLearnBonusData> skillLearnBonus = SkillHelper.GetSkillLearnBonus(skillId);
		if (skillLearnBonus.Count > 0)
		{
			for (int i = 0; i < skillLearnBonus.Count; i++)
			{
				totalPhysicalPoints += skillLearnBonus[i].Physical;
				totalMentalPoints += skillLearnBonus[i].Mental;
				totalMartialPoints += skillLearnBonus[i].Martial;
				totalSkillPoints += skillLearnBonus[i].Skill;
				totalSpellPoints += skillLearnBonus[i].Spell;
			}
		}
	}

	private string GetRandomName(WarbandId warbandId, UnitId unitId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = ((int)warbandId).ToConstantString();
		string text2 = ((int)unitId).ToConstantString();
		string[] fields = new string[3]
		{
			"fk_warband_id",
			"fk_unit_id",
			"surname"
		};
		string[] array = new string[3]
		{
			text,
			text2,
			"0"
		};
		List<UnitNameData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitNameData>(fields, array);
		if (list.Count > 0)
		{
			int index = Random.Range(0, list.Count - 1);
			stringBuilder.Append(list[index].TheName);
		}
		else
		{
			array[1] = "0";
			list = PandoraSingleton<DataFactory>.Instance.InitData<UnitNameData>(fields, array);
			if (list.Count > 0)
			{
				int index2 = Random.Range(0, list.Count - 1);
				stringBuilder.Append(list[index2].TheName);
			}
		}
		array[1] = text2;
		array[2] = "1";
		list = PandoraSingleton<DataFactory>.Instance.InitData<UnitNameData>(fields, array);
		if (list.Count > 0)
		{
			int index3 = Random.Range(0, list.Count - 1);
			stringBuilder.Append(" ");
			stringBuilder.Append(list[index3].TheName);
		}
		else
		{
			array[1] = "0";
			list = PandoraSingleton<DataFactory>.Instance.InitData<UnitNameData>(fields, array);
			if (list.Count > 0)
			{
				int index4 = Random.Range(0, list.Count - 1);
				stringBuilder.Append(" ");
				stringBuilder.Append(list[index4].TheName);
			}
		}
		return (stringBuilder.Length <= 0) ? "Edgar" : stringBuilder.ToString();
	}

	public Sprite GetIcon()
	{
		Sprite sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("unit/" + Id.ToLowerString(), cached: true);
		if (sprite == null)
		{
			PandoraDebug.LogWarning("Could not find icon at unit/" + Id.ToLowerString(), "UI");
		}
		return sprite;
	}

	public Sprite GetUnitTypeIcon()
	{
		switch (GetUnitTypeId())
		{
		case UnitTypeId.LEADER:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true);
		case UnitTypeId.HERO_1:
		case UnitTypeId.HERO_2:
		case UnitTypeId.HERO_3:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true);
		case UnitTypeId.IMPRESSIVE:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true);
		default:
			return null;
		}
	}

	public Sprite GetActiveStatusIcon()
	{
		switch (GetActiveStatus())
		{
		case UnitActiveStatusId.AVAILABLE:
			return null;
		case UnitActiveStatusId.TREATMENT_NOT_PAID:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("active_status/treatment", cached: true);
		case UnitActiveStatusId.INJURED:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("active_status/injured", cached: true);
		case UnitActiveStatusId.UPKEEP_NOT_PAID:
		case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("active_status/unpaid", cached: true);
		case UnitActiveStatusId.IN_TRAINING:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("active_status/training", cached: true);
		default:
			return null;
		}
	}

	public Color GetActiveStatusIconColor()
	{
		switch (GetActiveStatus())
		{
		case UnitActiveStatusId.AVAILABLE:
			return Color.white;
		case UnitActiveStatusId.UPKEEP_NOT_PAID:
		case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
			return Constant.GetColor(ConstantId.COLOR_GOLD);
		case UnitActiveStatusId.IN_TRAINING:
			return Constant.GetColor(ConstantId.COLOR_GREEN);
		case UnitActiveStatusId.INJURED:
		case UnitActiveStatusId.TREATMENT_NOT_PAID:
			return Constant.GetColor(ConstantId.COLOR_RED);
		default:
			return Color.white;
		}
	}

	public int GetActiveStatusUnits()
	{
		switch (GetActiveStatus())
		{
		case UnitActiveStatusId.AVAILABLE:
			return 0;
		case UnitActiveStatusId.UPKEEP_NOT_PAID:
		case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
			return GetUpkeepOwned();
		case UnitActiveStatusId.IN_TRAINING:
			return UnitSave.trainingTime;
		case UnitActiveStatusId.INJURED:
			return UnitSave.injuredTime;
		case UnitActiveStatusId.TREATMENT_NOT_PAID:
		{
			Tuple<int, EventLogger.LogEvent, int> tuple = Logger.FindLastEvent(EventLogger.LogEvent.NO_TREATMENT);
			return tuple.Item1 - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
		}
		default:
			return 0;
		}
	}

	public Sprite GetSecondActiveStatusIcon()
	{
		switch (GetActiveStatus())
		{
		case UnitActiveStatusId.AVAILABLE:
		case UnitActiveStatusId.INJURED:
		case UnitActiveStatusId.UPKEEP_NOT_PAID:
		case UnitActiveStatusId.IN_TRAINING:
			return null;
		case UnitActiveStatusId.TREATMENT_NOT_PAID:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("active_status/unpaid", cached: true);
		case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("active_status/injured", cached: true);
		default:
			return null;
		}
	}

	public Color GetSecondActiveStatusIconColor()
	{
		switch (GetActiveStatus())
		{
		case UnitActiveStatusId.AVAILABLE:
		case UnitActiveStatusId.INJURED:
		case UnitActiveStatusId.UPKEEP_NOT_PAID:
		case UnitActiveStatusId.IN_TRAINING:
			return Color.white;
		case UnitActiveStatusId.TREATMENT_NOT_PAID:
			return Constant.GetColor(ConstantId.COLOR_GOLD);
		case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
			return Constant.GetColor(ConstantId.COLOR_RED);
		default:
			return Color.white;
		}
	}

	public int GetSecondActiveStatusUnits()
	{
		switch (GetActiveStatus())
		{
		case UnitActiveStatusId.AVAILABLE:
		case UnitActiveStatusId.INJURED:
		case UnitActiveStatusId.UPKEEP_NOT_PAID:
		case UnitActiveStatusId.IN_TRAINING:
			return 0;
		case UnitActiveStatusId.TREATMENT_NOT_PAID:
			return PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitTreatmentCost(this);
		case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
			return UnitSave.injuredTime;
		default:
			return 0;
		}
	}

	public void Resurect()
	{
		CurrentWound = Wound;
		Status = UnitStateId.NONE;
	}

	public int AddToAttribute(AttributeId attributeId, int add)
	{
		int num = attributes[(int)attributeId] + add;
		attributes[(int)attributeId] = num;
		if (UnitSave.stats.stats.ContainsKey((int)attributeId))
		{
			Dictionary<int, int> stats;
			Dictionary<int, int> dictionary = stats = UnitSave.stats.stats;
			int key;
			int key2 = key = (int)attributeId;
			key = stats[key];
			dictionary[key2] = key + add;
		}
		return num;
	}

	public AttributeData GetAttributeData(AttributeId attributeId)
	{
		return attributeDataById[(int)attributeId];
	}

	public int GetAttribute(AttributeId attributeId)
	{
		return attributes[(int)attributeId];
	}

	public AttributeId GetAttributeModifierId(AttributeId attributeId)
	{
		AttributeData attributeData = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>((int)attributeId);
		return attributeData.AttributeIdModifier;
	}

	public int GetAttributeModifier(AttributeId attributeId)
	{
		AttributeId attributeModifierId = GetAttributeModifierId(attributeId);
		return (attributeModifierId != 0) ? GetAttribute(attributeModifierId) : 0;
	}

	public AttributeId GetMaxAttribute(AttributeId attributeId)
	{
		if (maxAttributes.TryGetValue((int)attributeId, out AttributeId value))
		{
			return value;
		}
		return AttributeId.NONE;
	}

	public void SetAttribute(AttributeId attributeId, int value)
	{
		attributes[(int)attributeId] = value;
		if (UnitSave.stats.stats.ContainsKey((int)attributeId))
		{
			UnitSave.stats.stats[(int)attributeId] = value;
		}
	}

	public int GetTempAttribute(AttributeId attributeId)
	{
		return tempAttributes[(int)attributeId];
	}

	public void SetTempAttribute(AttributeId attributeId, int value)
	{
		tempAttributes[(int)attributeId] = value;
	}

	public bool HasModifierType(AttributeId attributeId, AttributeMod.Type modifierType)
	{
		List<AttributeMod> orNull = attributeModifiers.GetOrNull(attributeId);
		if (orNull != null)
		{
			for (int i = 0; i < orNull.Count; i++)
			{
				AttributeMod attributeMod = orNull[i];
				if (attributeMod.type == modifierType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int AddToTempAttribute(AttributeId attributeId, int add)
	{
		int num = tempAttributes[(int)attributeId] + add;
		tempAttributes[(int)attributeId] = num;
		return num;
	}

	public bool HasPendingChanges()
	{
		for (int i = 0; i < tempAttributes.Length; i++)
		{
			if (tempAttributes[i] > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void ApplyChanges(bool update = true)
	{
		for (int i = 0; i < tempAttributes.Length; i++)
		{
			if (tempAttributes[i] > 0)
			{
				AttributeId key = (AttributeId)i;
				int num = tempAttributes[i];
				int value = 0;
				if (UnitSave.attributes.TryGetValue(key, out value))
				{
					UnitSave.attributes[key] = value + num;
				}
				else
				{
					UnitSave.attributes[key] = num;
				}
				tempAttributes[i] = 0;
			}
		}
		CalculateProgressionPoints();
		if (update)
		{
			UpdateAttributes();
		}
	}

	public void ResetChanges()
	{
		for (int i = 0; i < tempAttributes.Length; i++)
		{
			tempAttributes[i] = 0;
		}
		UpdateAttributes();
	}

	private void AddAttributeModifier(AttributeMod.Type modifierType, AttributeId attributeId, string reason, int modifier, bool isPercent = false)
	{
		attributeModifiers[attributeId].Add(new AttributeMod(modifierType, reason, modifier));
	}

	public void UpdateAttributesAndCheckBackPack(List<Item> removedItems)
	{
		int backpackCapacity = BackpackCapacity;
		UpdateAttributes();
		if (backpackCapacity <= BackpackCapacity)
		{
			return;
		}
		for (int i = 6 + BackpackCapacity; i < 6 + backpackCapacity; i++)
		{
			if (items[i].Id != 0)
			{
				removedItems.Add(items[i]);
				items[i] = new Item(ItemId.NONE);
			}
			UnitSave.items[i] = null;
		}
	}

	public void UpdateAttributes()
	{
		foreach (List<AttributeMod> value in attributeModifiers.Values)
		{
			value.Clear();
		}
		MovementData movementData = PandoraSingleton<DataFactory>.Instance.InitData<MovementData>((int)Data.MovementId);
		UnitWoundData unitWoundData = PandoraSingleton<DataFactory>.Instance.InitData<UnitWoundData>((int)Data.UnitWoundId);
		UnitTypeData unitTypeData = PandoraSingleton<DataFactory>.Instance.InitData<UnitTypeData>((int)GetUnitTypeId());
		UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)UnitSave.rankId);
		UnitRankData unitRankData2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>("rank", unitRankData.Rank.ToConstantString(), "advancement", "0")[0];
		for (int i = 0; i < attributeDataList.Count; i++)
		{
			AttributeData attributeData = attributeDataList[i];
			if (attributeData.Id == AttributeId.NONE || attributeData.Persistent)
			{
				continue;
			}
			if (attributeData.BaseRoll != 0)
			{
				if (attributeData.IsBaseRoll)
				{
					AddAttributeModifier(AttributeMod.Type.BASE, attributeData.Id, PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_base_roll"), attributeData.BaseRoll, isPercent: true);
				}
				else
				{
					AddAttributeModifier(AttributeMod.Type.BASE, attributeData.Id, PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("attribute_", attributeData.Name), attributeData.BaseRoll, isPercent: true);
				}
			}
			SetAttribute(attributeData.Id, attributeData.BaseRoll);
		}
		SetAttribute(AttributeId.MORAL_IMPACT, baseMoralImpact);
		UnitAlwaysVisible = false;
		newState = Status;
		AddToAttribute(AttributeId.MOVEMENT, movementData.Distance);
		AddToAttribute(AttributeId.STRATEGY_POINTS, unitTypeData.StartSp);
		AddToAttribute(AttributeId.OFFENSE_POINTS, unitTypeData.StartOp);
		AddToAttribute(AttributeId.WOUND, unitWoundData.BaseWound + unitRankData2.Wound);
		AddToAttribute(AttributeId.INITIATIVE, unitTypeData.InitiativeBonus);
		SetAttribute(AttributeId.DAMAGE_MIN, Items[(int)ActiveWeaponSlot].DamageMin);
		SetAttribute(AttributeId.DAMAGE_MAX, Items[(int)ActiveWeaponSlot].DamageMax);
		for (int j = 0; j < baseAttributesData.Count; j++)
		{
			UnitJoinAttributeData unitJoinAttributeData = baseAttributesData[j];
			AddToAttribute(unitJoinAttributeData.AttributeId, unitJoinAttributeData.BaseValue);
		}
		foreach (KeyValuePair<AttributeId, int> attribute2 in UnitSave.attributes)
		{
			AddToAttribute(attribute2.Key, attribute2.Value);
		}
		for (int k = 0; k < ranksData.Count && ranksData[k].UnitRankId <= UnitSave.rankId; k++)
		{
			AddToAttribute(AttributeId.WOUND, ranksData[k].Wound);
			AddToAttribute(AttributeId.STRATEGY_POINTS, ranksData[k].Strategy);
			AddToAttribute(AttributeId.OFFENSE_POINTS, ranksData[k].Offense);
		}
		if (CampaignData != null)
		{
			for (int l = 0; l < campaignModifiers.Count; l++)
			{
				CampaignUnitJoinAttributeData campaignUnitJoinAttributeData = campaignModifiers[l];
				AddToAttribute(campaignUnitJoinAttributeData.AttributeId, campaignUnitJoinAttributeData.Value);
			}
		}
		for (int m = 0; m < tempAttributes.Length; m++)
		{
			AttributeId attributeId = (AttributeId)m;
			if (GetTempAttribute(attributeId) > 0)
			{
				AddToAttribute((AttributeId)m, tempAttributes[m]);
				AddAttributeModifier(AttributeMod.Type.TEMP, (AttributeId)m, "temp", tempAttributes[m]);
			}
		}
		AddToAttribute(AttributeId.ARMOR_ABSORPTION, Items[0].ArmorAbsorption + Items[1].ArmorAbsorption);
		if (Items[0].ArmorAbsorption > 0)
		{
			AddAttributeModifier(AttributeMod.Type.ITEM, AttributeId.ARMOR_ABSORPTION, PandoraSingleton<LocalizationManager>.Instance.GetStringById(Items[0].LabelName), Items[0].ArmorAbsorption);
		}
		if (Items[1].ArmorAbsorption > 0)
		{
			AddAttributeModifier(AttributeMod.Type.ITEM, AttributeId.ARMOR_ABSORPTION, PandoraSingleton<LocalizationManager>.Instance.GetStringById(Items[1].LabelName), Items[1].ArmorAbsorption);
		}
		actionCostModifiers.Clear();
		skillCostModifiers.Clear();
		spellTypeModifiers.Clear();
		damagePercModifiers.Clear();
		blockedActions.Clear();
		blockedSkills.Clear();
		blockedBones.Clear();
		blockedItemTypes.Clear();
		if (Items[(int)(ActiveWeaponSlot + 1)].Id != 0 && Items[(int)(ActiveWeaponSlot + 1)].DamageMin > 0)
		{
			AddToAttribute(AttributeId.DAMAGE_MIN, Items[(int)(ActiveWeaponSlot + 1)].DamageMin);
			AddToAttribute(AttributeId.DAMAGE_MAX, Items[(int)(ActiveWeaponSlot + 1)].DamageMax);
		}
		RefreshActiveItems();
		for (int n = 0; n < PassiveSkills.Count; n++)
		{
			ApplySkill(PassiveSkills[n]);
		}
		for (int num = 0; num < ConsumableSkills.Count; num++)
		{
			ApplySkill(ConsumableSkills[num]);
		}
		if (ActiveSkill != null)
		{
			ApplySkill(ActiveSkill);
		}
		RefreshImmunities();
		for (int num2 = Enchantments.Count - 1; num2 >= 0; num2--)
		{
			bool flag = false;
			if (!Enchantments[num2].Data.Indestructible)
			{
				int num3 = enchantTypeImmunities.IndexOf(Enchantments[num2].Data.EnchantmentTypeId, EnchantmentTypeIdComparer.Instance);
				int num4 = enchantTypeToBeRemoved.IndexOf(Enchantments[num2].Data.EnchantmentTypeId, EnchantmentTypeIdComparer.Instance);
				int num5 = enchantToBeRemoved.IndexOf(Enchantments[num2].Id, EnchantmentIdComparer.Instance);
				if ((Enchantments[num2].Data.RequireUnitState && Enchantments[num2].Data.UnitStateIdRequired != Status) || num3 != -1 || num4 != -1 || num5 != -1)
				{
					flag = true;
					RemoveEnchantment(num2);
					if (num4 != -1)
					{
						enchantTypeToBeRemoved.RemoveAt(num4);
					}
					else if (num5 != -1)
					{
						enchantToBeRemoved.RemoveAt(num5);
					}
				}
			}
			if (!flag)
			{
				ApplyEnchantment(Enchantments[num2]);
			}
		}
		for (int num6 = 0; num6 < ActiveItems.Count; num6++)
		{
			ApplyItem(ActiveItems[num6]);
			if (ActiveItems[num6].SpeedData != null)
			{
				AddToAttribute(AttributeId.INITIATIVE, ActiveItems[num6].SpeedData.Speed);
			}
		}
		for (int num7 = 0; num7 < Injuries.Count; num7++)
		{
			ApplyInjury(Injuries[num7]);
		}
		for (int num8 = 0; num8 < Mutations.Count; num8++)
		{
			ApplyMutation(Mutations[num8]);
		}
		RemoveAppliedEnchantments();
		for (int num9 = 0; num9 < baseAttributesDataMax.Count; num9++)
		{
			UnitTypeAttributeData unitTypeAttributeData = baseAttributesDataMax[num9];
			int attribute = GetAttribute(unitTypeAttributeData.AttributeId);
			attribute = Mathf.Min(attribute, unitTypeAttributeData.Max);
			SetAttribute(unitTypeAttributeData.AttributeId, attribute);
		}
		for (int num10 = 0; num10 < attributeDataList.Count; num10++)
		{
			AttributeData attributeData2 = attributeDataList[num10];
			if (attributeData2.Id != 0)
			{
				int num11 = GetAttribute(attributeData2.Id) + BaseStatIncrease(attributeData2.Id, attributeData2.IsPercent);
				if (attributeData2.AttributeIdMax != 0)
				{
					num11 = Mathf.Clamp(num11, 1, GetAttribute(attributeData2.AttributeIdMax));
				}
				if (attributeData2.Id == AttributeId.CRIT_RESISTANCE || attributeData2.Id == AttributeId.MORAL_IMPACT)
				{
					num11 = Mathf.Max(0, num11);
				}
				if (attributeData2.Id == AttributeId.ARMOR_ABSORPTION_PERC)
				{
					num11 = Mathf.Min(num11, 95);
				}
				SetAttribute(attributeData2.Id, num11);
			}
		}
		SetAttribute(AttributeId.WOUND, (int)((float)Wound * (1f + (float)WoundPerc / 100f)));
		attributeModifiers[AttributeId.WOUND].AddRange(attributeModifiers[AttributeId.WOUND_PERC]);
		CurrentWound = Mathf.Clamp(CurrentWound, 0, Wound);
		SetAttribute(AttributeId.STRATEGY_POINTS, Mathf.Clamp(GetAttribute(AttributeId.STRATEGY_POINTS), 0, unitTypeData.MaxSp));
		SetAttribute(AttributeId.OFFENSE_POINTS, Mathf.Clamp(GetAttribute(AttributeId.OFFENSE_POINTS), 0, unitTypeData.MaxOp));
		SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, Mathf.Clamp(CurrentStrategyPoints, 0, Mathf.Max(StrategyPoints, 0)));
		SetAttribute(AttributeId.CURRENT_OFFENSE_POINTS, Mathf.Clamp(CurrentOffensePoints, 0, Mathf.Max(OffensePoints, 0)));
		if (newState != Status)
		{
			SetStatus(newState);
			newState = UnitStateId.NONE;
		}
		if (needFxRefresh)
		{
			UpdateEnchantmentsFx();
		}
		realBackPackCapacity = Constant.GetInt(ConstantId.UNIT_BACKPACK_SLOT) + ((Strength >= Constant.GetInt(ConstantId.UNIT_BACKPACK_SLOT_STR_INCREASE_1)) ? 1 : 0) + ((Strength >= Constant.GetInt(ConstantId.UNIT_BACKPACK_SLOT_STR_INCREASE_2)) ? 1 : 0) + ((Strength >= Constant.GetInt(ConstantId.UNIT_BACKPACK_SLOT_STR_INCREASE_3)) ? 1 : 0) + ((Strength >= Constant.GetInt(ConstantId.UNIT_BACKPACK_SLOT_STR_INCREASE_4)) ? 1 : 0) + ((Strength >= Constant.GetInt(ConstantId.UNIT_BACKPACK_SLOT_STR_INCREASE_5)) ? 1 : 0);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ATTRIBUTES_CHANGED, this);
	}

	private void RefreshImmunities()
	{
		enchantTypeImmunities.Clear();
		enchantTypeToBeRemoved.Clear();
		enchantToBeRemoved.Clear();
		GetEnchantmentsImmunitiesRemovers(Enchantments);
		for (int i = 0; i < ActiveItems.Count; i++)
		{
			GetEnchantmentsImmunitiesRemovers(ActiveItems[i].Enchantments);
			if (ActiveItems[i].RuneMark != null)
			{
				GetEnchantmentsImmunitiesRemovers(ActiveItems[i].RuneMark.Enchantments);
			}
		}
		for (int j = 0; j < Injuries.Count; j++)
		{
			GetEnchantmentsImmunitiesRemovers(Injuries[j].Enchantments);
		}
		for (int k = 0; k < Mutations.Count; k++)
		{
			GetEnchantmentsImmunitiesRemovers(Mutations[k].Enchantments);
		}
	}

	public bool HasEnchantmentImmunity(Enchantment enchantment)
	{
		return HasEnchantmentImmunity(enchantment.Data.EnchantmentTypeId, enchantment.Id);
	}

	public bool HasEnchantmentImmunity(EnchantmentTypeId typeId, EnchantmentId id)
	{
		return enchantTypeImmunities.Contains(typeId, EnchantmentTypeIdComparer.Instance) || enchantTypeToBeRemoved.Contains(typeId, EnchantmentTypeIdComparer.Instance) || enchantToBeRemoved.Contains(id, EnchantmentIdComparer.Instance);
	}

	private int BaseStatIncrease(AttributeId attrId, bool isPercent)
	{
		int num = 0;
		if (attributeAttributeDataById.TryGetValue(attrId, out List<AttributeAttributeData> value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				AttributeAttributeData attributeAttributeData = value[i];
				if (attributeAttributeData != null)
				{
					int num2 = GetAttribute(attributeAttributeData.AttributeIdBase) * attributeAttributeData.Modifier;
					num += num2;
					AddAttributeModifier(AttributeMod.Type.ATTRIBUTE, attrId, PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("attribute_name_", GetAttributeData(attributeAttributeData.AttributeIdBase).Name), num2, isPercent);
					int tempAttribute = GetTempAttribute(attributeAttributeData.AttributeIdBase);
					if (tempAttribute > 0)
					{
						AddAttributeModifier(AttributeMod.Type.TEMP, attrId, "temp", tempAttribute, isPercent);
					}
				}
			}
		}
		return num;
	}

	private void AddUnitCostModifier(UnitActionId id, int sp, int op)
	{
		if (!actionCostModifiers.ContainsKey(id))
		{
			actionCostModifiers[id] = new CostModifier();
		}
		actionCostModifiers[id].strat += sp;
		actionCostModifiers[id].off += op;
	}

	private void AddSkillCostModifier(SkillId id, int sp, int op)
	{
		if (!skillCostModifiers.ContainsKey(id))
		{
			skillCostModifiers[id] = new CostModifier();
		}
		skillCostModifiers[id].strat += sp;
		skillCostModifiers[id].off += op;
	}

	private void AddSpellTypeCostMofier(SpellTypeId id, int sp, int op)
	{
		if (!spellTypeModifiers.ContainsKey(id))
		{
			spellTypeModifiers[id] = new CostModifier();
		}
		spellTypeModifiers[id].strat += sp;
		spellTypeModifiers[id].off += op;
	}

	private void AddDamagePercModifier(SkillId id, int damage)
	{
		if (!damagePercModifiers.ContainsKey(id))
		{
			damagePercModifiers[id] = 0;
		}
		Dictionary<SkillId, int> dictionary;
		Dictionary<SkillId, int> dictionary2 = dictionary = damagePercModifiers;
		SkillId key;
		SkillId key2 = key = id;
		int num = dictionary[key];
		dictionary2[key2] = num + damage;
	}

	private void GetEnchantmentsImmunitiesRemovers(List<Enchantment> enchantments)
	{
		for (int i = 0; i < enchantments.Count; i++)
		{
			Enchantment enchantment = enchantments[i];
			for (int j = 0; j < enchantment.Immunities.Count; j++)
			{
				EnchantmentRemoveEnchantmentTypeData enchantmentRemoveEnchantmentTypeData = enchantment.Immunities[j];
				if (enchantmentRemoveEnchantmentTypeData.EnchantmentTypeId == EnchantmentTypeId.NONE)
				{
					continue;
				}
				if (enchantmentRemoveEnchantmentTypeData.Count == 0)
				{
					enchantTypeImmunities.Add(enchantmentRemoveEnchantmentTypeData.EnchantmentTypeId);
					continue;
				}
				for (int k = 0; k < enchantmentRemoveEnchantmentTypeData.Count; k++)
				{
					enchantTypeToBeRemoved.Add(enchantmentRemoveEnchantmentTypeData.EnchantmentTypeId);
				}
			}
			for (int l = 0; l < enchantment.Remover.Count; l++)
			{
				EnchantmentRemoveEnchantmentData enchantmentRemoveEnchantmentData = enchantment.Remover[l];
				if (enchantmentRemoveEnchantmentData.EnchantmentIdRemove != 0)
				{
					for (int m = 0; m < enchantmentRemoveEnchantmentData.Count; m++)
					{
						enchantToBeRemoved.Add(enchantmentRemoveEnchantmentData.EnchantmentIdRemove);
					}
				}
			}
		}
	}

	private void SetMoralImpact()
	{
		UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)UnitSave.rankId);
		UnitTypeData unitTypeData = PandoraSingleton<DataFactory>.Instance.InitData<UnitTypeData>((int)GetUnitTypeId());
		baseMoralImpact = unitTypeData.MoralImpact + unitRankData.Rank;
	}

	private void ApplySkill(SkillData skillData)
	{
		string id = ((int)skillData.Id).ToConstantString();
		List<SkillItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillItemData>("fk_skill_id", ((int)skillData.Id).ToConstantString());
		if (list.Count > 0)
		{
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (flag)
				{
					break;
				}
				SkillItemData skillItemData = list[i];
				flag = HasItemActive(skillItemData.ItemId);
				if (flag && skillItemData.MutationId != 0)
				{
					flag = HasMutation(skillItemData.MutationId);
				}
			}
			if (!flag)
			{
				return;
			}
		}
		List<SkillAttributeData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<SkillAttributeData>("fk_skill_id", id);
		for (int j = 0; j < list2.Count; j++)
		{
			SkillAttributeData skillAttributeData = list2[j];
			if ((skillAttributeData.UnitActionIdTrigger == UnitActionId.NONE && skillAttributeData.SkillIdTrigger == SkillId.NONE) || (ActiveSkill != null && ActiveSkill.UnitActionId != 0 && ActiveSkill.UnitActionId == skillAttributeData.UnitActionIdTrigger) || (ActiveSkill != null && ActiveSkill.Id != 0 && ActiveSkill.Id == skillAttributeData.SkillIdTrigger))
			{
				AddToAttribute(skillAttributeData.AttributeId, skillAttributeData.Modifier);
				AddAttributeModifier(AttributeMod.Type.SKILL, skillAttributeData.AttributeId, PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("skill_name_", skillAttributeData.SkillId.ToSkillIdString()), skillAttributeData.Modifier);
			}
		}
		List<SkillCostModifierData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<SkillCostModifierData>("fk_skill_id", id);
		for (int k = 0; k < list3.Count; k++)
		{
			SkillCostModifierData skillCostModifierData = list3[k];
			if (skillCostModifierData.UnitActionId != 0)
			{
				AddUnitCostModifier(skillCostModifierData.UnitActionId, skillCostModifierData.StrategyPoints, skillCostModifierData.OffensePoints);
			}
			else if (skillCostModifierData.SkillId != 0)
			{
				AddSkillCostModifier(skillCostModifierData.SkillIdTarget, skillCostModifierData.StrategyPoints, skillCostModifierData.OffensePoints);
			}
		}
	}

	public bool HasPassiveSkill(SkillId skillId)
	{
		for (int i = 0; i < PassiveSkills.Count; i++)
		{
			SkillData skillData = PassiveSkills[i];
			if (skillData.Id == skillId || skillData.SkillIdPrerequiste == skillId)
			{
				return true;
			}
		}
		return false;
	}

	private int RemoveSkillFromList(List<SkillData> skillsList, SkillId skillId)
	{
		for (int i = 0; i < skillsList.Count; i++)
		{
			if (skillsList[i].Id == skillId)
			{
				RemoveEnchantments(skillId, this);
				skillsList.RemoveAt(i);
				return i;
			}
		}
		return -1;
	}

	private void AddSkill(SkillId skillId, bool updateAttributes = true)
	{
		SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)skillId);
		int num = RemovePrequesiteSkill(skillData);
		if (skillData.SkillTypeId == SkillTypeId.CONSUMABLE_ACTION)
		{
			if (num == -1)
			{
				num = ConsumableSkills.Count;
			}
			ConsumableSkills.Insert(num, skillData);
			AddSkillEnchantment(skillData);
			CalculateProgressionPoints();
		}
		else if (skillData.Passive)
		{
			if (num == -1)
			{
				num = PassiveSkills.Count;
			}
			PassiveSkills.Insert(num, skillData);
			AddSkillEnchantment(skillData);
		}
		else if (skillData.SpellTypeId != 0)
		{
			if (num == -1)
			{
				num = Spells.Count;
			}
			Spells.Insert(num, skillData);
		}
		else
		{
			if (num == -1)
			{
				num = ActiveSkills.Count;
			}
			ActiveSkills.Insert(num, skillData);
		}
		if (updateAttributes)
		{
			UpdateAttributes();
		}
	}

	private int RemovePrequesiteSkill(SkillData skillData)
	{
		int result = -1;
		if (skillData.SkillTypeId != SkillTypeId.CONSUMABLE_ACTION && SkillHelper.IsMastery(skillData) && skillData.SkillIdPrerequiste != 0)
		{
			result = (skillData.Passive ? RemoveSkillFromList(PassiveSkills, skillData.SkillIdPrerequiste) : ((skillData.SkillTypeId != SkillTypeId.SPELL_ACTION) ? RemoveSkillFromList(ActiveSkills, skillData.SkillIdPrerequiste) : RemoveSkillFromList(Spells, skillData.SkillIdPrerequiste)));
		}
		return result;
	}

	private void AddSkillEnchantment(SkillData skillData)
	{
		List<SkillEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillEnchantmentData>("fk_skill_id", ((int)skillData.Id).ToConstantString());
		for (int i = 0; i < list.Count; i++)
		{
			SkillEnchantmentData skillEnchantmentData = list[i];
			if (skillEnchantmentData.EnchantmentTriggerId == EnchantmentTriggerId.NONE && skillEnchantmentData.Self)
			{
				AddEnchantment(skillEnchantmentData.EnchantmentId, this, skillData.Passive, updateAttributes: false);
			}
		}
	}

	public void SetActiveSkill(SkillData data)
	{
		ActiveSkill = data;
		UpdateAttributes();
	}

	public bool NeedsRoll(AttributeId attributeId)
	{
		if (HasEnchantmentRollEffect(Enchantments, attributeId))
		{
			return true;
		}
		for (int i = 0; i < ActiveItems.Count; i++)
		{
			if (HasEnchantmentRollEffect(ActiveItems[i].Enchantments, attributeId))
			{
				return true;
			}
			if (ActiveItems[i].RuneMark != null && HasEnchantmentRollEffect(ActiveItems[i].RuneMark.Enchantments, attributeId))
			{
				return true;
			}
		}
		for (int j = 0; j < Mutations.Count; j++)
		{
			if (HasEnchantmentRollEffect(Mutations[j].Enchantments, attributeId))
			{
				return true;
			}
		}
		for (int k = 0; k < Injuries.Count; k++)
		{
			if (HasEnchantmentRollEffect(Injuries[k].Enchantments, attributeId))
			{
				return true;
			}
		}
		return false;
	}

	public bool Roll(Tyche tyche, AttributeId attributeId, bool reverse = false, bool apply = true)
	{
		return Roll(tyche, GetAttribute(attributeId), attributeId, reverse, apply);
	}

	public bool Roll(Tyche tyche, int target, AttributeId attributeId, bool reverse = false, bool apply = true, int mod = 0)
	{
		target = Mathf.Clamp(target, 0, 100);
		int num = tyche.Rand(0, 100);
		num = Mathf.Max(num - mod, 0);
		bool flag = num < target;
		bool flag2 = (!reverse) ? flag : (!flag);
		UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
		if (currentUnit != null)
		{
			Unit unit = currentUnit.unit;
			if (this == unit)
			{
				PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, (!flag2) ? CombatLogger.LogMessage.ROLL_FAIL : CombatLogger.LogMessage.ROLL_SUCCESS, PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_" + attributeId.ToAttributeIdString()), (num + 1).ToConstantString(), target.ToConstantString());
			}
			else
			{
				PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, (!flag2) ? CombatLogger.LogMessage.UNIT_ROLL_FAIL : CombatLogger.LogMessage.UNIT_ROLL_SUCCESS, PandoraSingleton<MissionManager>.Instance.GetUnitController(this).GetLogName(), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_" + attributeId.ToAttributeIdString()), (num + 1).ToConstantString(), target.ToConstantString());
			}
		}
		if (apply)
		{
			ApplyRollResult(tyche, num, flag, attributeId);
		}
		return flag;
	}

	public void ApplyRollResult(Tyche tyche, int rand, bool success, AttributeId attrId)
	{
		bool flag = false;
		flag |= SetEnchantRollResult(tyche, Enchantments, rand, success, attrId);
		for (int i = 0; i < ActiveItems.Count; i++)
		{
			flag |= SetEnchantRollResult(tyche, ActiveItems[i].Enchantments, rand, success, attrId);
			if (ActiveItems[i].RuneMark != null)
			{
				flag |= SetEnchantRollResult(tyche, ActiveItems[i].RuneMark.Enchantments, rand, success, attrId);
			}
		}
		for (int j = 0; j < Injuries.Count; j++)
		{
			flag |= SetEnchantRollResult(tyche, Injuries[j].Enchantments, rand, success, attrId);
		}
		for (int k = 0; k < Mutations.Count; k++)
		{
			flag |= SetEnchantRollResult(tyche, Mutations[k].Enchantments, rand, success, attrId);
		}
		if (flag)
		{
			UpdateAttributes();
		}
		if (flag)
		{
			UpdateAttributes();
		}
	}

	private bool SetEnchantRollResult(Tyche tyche, List<Enchantment> enchantments, int rand, bool success, AttributeId attrId)
	{
		bool result = false;
		for (int num = enchantments.Count - 1; num >= 0; num--)
		{
			for (int i = 0; i < enchantments[num].Effects.Count; i++)
			{
				EnchantmentEffectEnchantmentData enchantmentEffectEnchantmentData = enchantments[num].Effects[i];
				if (enchantmentEffectEnchantmentData.AttributeIdRoll != attrId || ((!success || enchantmentEffectEnchantmentData.EnchantmentTriggerId != EnchantmentTriggerId.ON_ROLL_SUCCESS) && (success || enchantmentEffectEnchantmentData.EnchantmentTriggerId != EnchantmentTriggerId.ON_ROLL_FAIL)))
				{
					continue;
				}
				bool flag = true;
				if (enchantmentEffectEnchantmentData.Ratio != 0)
				{
					flag = (tyche.Rand(0, 100) < enchantmentEffectEnchantmentData.Ratio);
				}
				if (!flag)
				{
					continue;
				}
				Enchantment enchantment = AddEnchantment(enchantmentEffectEnchantmentData.EnchantmentIdEffect, (enchantments[num].Provider == null) ? this : enchantments[num].Provider, original: false, updateAttributes: false, enchantments[num].AllegianceId);
				if (enchantment != null)
				{
					if (!enchantment.Data.NoDisplay)
					{
						PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_ENCHANTMENT, this, enchantment.LocalizedName, enchantment.Data.EffectTypeId);
					}
					result = true;
				}
			}
		}
		return result;
	}

	public void ResetEnchantsChanged()
	{
		HasEnchantmentsChanged = false;
	}

	private void ApplyEnchantment(Enchantment enchant)
	{
		if (enchantTypeImmunities.IndexOf(enchant.Data.EnchantmentTypeId, EnchantmentTypeIdComparer.Instance) != -1 || (enchant.Data.RequireUnitState && Status != enchant.Data.UnitStateIdRequired))
		{
			return;
		}
		for (int i = 0; i < enchant.AttributeModifiers.Count; i++)
		{
			EnchantmentJoinAttributeData enchantmentJoinAttributeData = enchant.AttributeModifiers[i];
			AddToAttribute(enchantmentJoinAttributeData.AttributeId, enchantmentJoinAttributeData.Modifier);
			if (enchantmentJoinAttributeData.AttributeId == AttributeId.CURRENT_WOUND && enchantmentJoinAttributeData.Modifier != 0 && PandoraSingleton<MissionManager>.Exists())
			{
				PandoraSingleton<MissionManager>.Instance.GetUnitController(this).ComputeDirectWound(-enchantmentJoinAttributeData.Modifier, byPassArmor: true, PandoraSingleton<MissionManager>.Instance.GetUnitController(enchant.Provider), fake: true);
			}
			if (!enchant.Data.NoDisplay)
			{
				AttributeMod.Type modifierType = AttributeMod.Type.ENCHANTMENT;
				switch (enchant.Data.EffectTypeId)
				{
				case EffectTypeId.BUFF:
					modifierType = AttributeMod.Type.BUFF;
					break;
				case EffectTypeId.DEBUFF:
					modifierType = AttributeMod.Type.DEBUFF;
					break;
				}
				AddAttributeModifier(modifierType, enchantmentJoinAttributeData.AttributeId, PandoraSingleton<LocalizationManager>.Instance.GetStringById(enchant.LabelName), enchantmentJoinAttributeData.Modifier);
			}
		}
		for (int j = 0; j < enchant.CostModifiers.Count; j++)
		{
			EnchantmentCostModifierData enchantmentCostModifierData = enchant.CostModifiers[j];
			if (enchantmentCostModifierData.UnitActionId != 0)
			{
				AddUnitCostModifier(enchantmentCostModifierData.UnitActionId, enchantmentCostModifierData.StrategyPoints, enchantmentCostModifierData.OffensePoints);
			}
			else if (enchantmentCostModifierData.SkillId != 0)
			{
				AddSkillCostModifier(enchantmentCostModifierData.SkillId, enchantmentCostModifierData.StrategyPoints, enchantmentCostModifierData.OffensePoints);
			}
			else if (enchantmentCostModifierData.SpellTypeId != 0)
			{
				AddSpellTypeCostMofier(enchantmentCostModifierData.SpellTypeId, enchantmentCostModifierData.StrategyPoints, enchantmentCostModifierData.OffensePoints);
			}
		}
		for (int k = 0; k < enchant.DamageModifiers.Count; k++)
		{
			EnchantmentDamageModifierData enchantmentDamageModifierData = enchant.DamageModifiers[k];
			AddDamagePercModifier(enchantmentDamageModifierData.SkillId, enchantmentDamageModifierData.DamagePercModifier);
		}
		for (int l = 0; l < enchant.ActionBlockers.Count; l++)
		{
			EnchantmentBlockUnitActionData enchantmentBlockUnitActionData = enchant.ActionBlockers[l];
			if (enchantmentBlockUnitActionData.UnitActionId != 0 && !blockedActions.ContainsKey(enchantmentBlockUnitActionData.UnitActionId))
			{
				blockedActions.Add(enchantmentBlockUnitActionData.UnitActionId, enchant.Id);
			}
			if (enchantmentBlockUnitActionData.SkillId != 0 && !blockedSkills.ContainsKey(enchantmentBlockUnitActionData.SkillId))
			{
				blockedSkills.Add(enchantmentBlockUnitActionData.SkillId, enchant.Id);
			}
		}
		for (int m = 0; m < enchant.BoneBlockers.Count; m++)
		{
			EnchantmentBlockBoneData enchantmentBlockBoneData = enchant.BoneBlockers[m];
			if (!blockedBones.ContainsKey(enchantmentBlockBoneData.BoneId))
			{
				blockedBones.Add(enchantmentBlockBoneData.BoneId, enchantmentBlockBoneData.EnchantmentId);
			}
		}
		for (int n = 0; n < enchant.ItemTypeBlockers.Count; n++)
		{
			EnchantmentBlockItemTypeData enchantmentBlockItemTypeData = enchant.ItemTypeBlockers[n];
			if (!blockedItemTypes.ContainsKey(enchantmentBlockItemTypeData.ItemTypeId))
			{
				blockedItemTypes.Add(enchantmentBlockItemTypeData.ItemTypeId, enchantmentBlockItemTypeData.EnchantmentId);
			}
		}
		if (enchant.Data.ChangeUnitState && enchant.Data.UnitStateIdRequired == Status)
		{
			newState = enchant.Data.UnitStateIdNext;
			stunningUnit = enchant.Provider;
		}
		if (enchant.Data.MakeUnitVisible)
		{
			UnitAlwaysVisible = true;
		}
	}

	public bool HasEnchantment(EnchantmentId enchantmentId)
	{
		EnchantmentData enchantmentData = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentData>((int)enchantmentId);
		if (enchantTypeImmunities.IndexOf(enchantmentData.EnchantmentTypeId, EnchantmentTypeIdComparer.Instance) != -1)
		{
			return false;
		}
		bool flag = Items[(int)ActiveWeaponSlot].HasEnchantment(enchantmentId) || Items[(int)(ActiveWeaponSlot + 1)].HasEnchantment(enchantmentId);
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (Enchantments[i].Id == enchantmentId)
			{
				flag = true;
			}
		}
		return flag;
	}

	public bool HasEnchantment(EnchantmentTypeId typeId)
	{
		if (enchantTypeImmunities.IndexOf(typeId, EnchantmentTypeIdComparer.Instance) != -1)
		{
			return false;
		}
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (Enchantments[i].Data.EnchantmentTypeId == typeId)
			{
				return true;
			}
		}
		for (int j = 0; j < ActiveItems.Count; j++)
		{
			for (int k = 0; k < ActiveItems[j].Enchantments.Count; k++)
			{
				if (ActiveItems[j].Enchantments[k].Data.EnchantmentTypeId == typeId)
				{
					return true;
				}
			}
		}
		for (int l = 0; l < Injuries.Count; l++)
		{
			for (int m = 0; m < Injuries[l].Enchantments.Count; m++)
			{
				if (Injuries[l].Enchantments[m].Data.EnchantmentTypeId == typeId)
				{
					return true;
				}
			}
		}
		for (int n = 0; n < Mutations.Count; n++)
		{
			for (int num = 0; num < Mutations[n].Enchantments.Count; num++)
			{
				if (Mutations[n].Enchantments[num].Data.EnchantmentTypeId == typeId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEnchantmentRollEffect(List<Enchantment> enchantments, AttributeId attributeId)
	{
		for (int i = 0; i < enchantments.Count; i++)
		{
			for (int j = 0; j < enchantments[i].Effects.Count; j++)
			{
				if (enchantments[i].Effects[j].AttributeIdRoll == attributeId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Enchantment AddEnchantment(EnchantmentId id, Unit provider, bool original, bool updateAttributes = true, AllegianceId allegianceId = AllegianceId.NONE)
	{
		RefreshImmunities();
		EnchantmentData enchantmentData = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentData>((int)id);
		if (enchantmentData.RequireUnitState && Status != enchantmentData.UnitStateIdRequired)
		{
			return null;
		}
		if (HasEnchantmentImmunity(enchantmentData.EnchantmentTypeId, enchantmentData.Id))
		{
			return null;
		}
		int num = -1;
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (Enchantments[i].Id == id)
			{
				num = i;
			}
		}
		if (!enchantmentData.Stackable && num != -1)
		{
			RemoveEnchantment(num);
		}
		Enchantment enchantment = new Enchantment(id, this, provider, original, innate: false, allegianceId, num == -1);
		Enchantments.Add(enchantment);
		if (PandoraSingleton<MissionManager>.Exists())
		{
			UnitController unitController = PandoraSingleton<MissionManager>.Instance.GetUnitController(this);
			PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(unitController);
		}
		if (updateAttributes)
		{
			UpdateAttributes();
		}
		HasEnchantmentsChanged = true;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ENCHANTMENTS_CHANGED, this);
		return enchantment;
	}

	public void ConsumeEnchantments(EnchantmentConsumeId consumeId)
	{
		bool flag = false;
		List<EnchantmentId> list = new List<EnchantmentId>();
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].Data.EnchantmentConsumeId == consumeId && list.IndexOf(Enchantments[num].Id, EnchantmentIdComparer.Instance) == -1)
			{
				list.Add(Enchantments[num].Id);
				RemoveEnchantment(num);
				flag = true;
			}
		}
		if (flag)
		{
			UpdateAttributes();
		}
		HasEnchantmentsChanged = true;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ENCHANTMENTS_CHANGED, this);
	}

	public void DestroyEnchantments(EnchantmentTriggerId triggerId, bool updateAttributes = false)
	{
		bool flag = false;
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (Enchantments[i].Data.EnchantmentTriggerIdDestroy == triggerId)
			{
				flag = true;
				RemoveEnchantment(i);
			}
		}
		if (flag && updateAttributes)
		{
			UpdateAttributes();
		}
		HasEnchantmentsChanged = true;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ENCHANTMENTS_CHANGED, this);
	}

	public void RemoveEnchantments(SkillId skillId, Unit unit)
	{
		List<SkillEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillEnchantmentData>("fk_skill_id", ((int)skillId).ToConstantString());
		for (int i = 0; i < list.Count; i++)
		{
			RemoveEnchantment(list[i].EnchantmentId, unit);
		}
	}

	public void RemoveEnchantments(EnchantmentTypeId typeId)
	{
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].Data.EnchantmentTypeId == typeId)
			{
				RemoveEnchantment(num);
			}
		}
	}

	public void RemoveEnchantments(EnchantmentId id)
	{
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].Id == id)
			{
				RemoveEnchantment(num);
			}
		}
	}

	public void RemoveEnchantment(EnchantmentId id, Unit unit)
	{
		int num = Enchantments.Count - 1;
		while (true)
		{
			if (num >= 0)
			{
				if (Enchantments[num].Id == id && (unit == null || Enchantments[num].Provider == unit))
				{
					break;
				}
				num--;
				continue;
			}
			return;
		}
		RemoveEnchantment(num);
	}

	public void RemoveEnchantment(int i)
	{
		Enchantments[i].RemoveFx();
		Enchantments.RemoveAt(i);
		needFxRefresh = true;
		if (PandoraSingleton<MissionManager>.Exists())
		{
			UnitController unitController = PandoraSingleton<MissionManager>.Instance.GetUnitController(this);
			PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(unitController);
		}
		HasEnchantmentsChanged = true;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ENCHANTMENTS_CHANGED, this);
	}

	public void RemoveAppliedEnchantments()
	{
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].Data.DestroyOnApply)
			{
				RemoveEnchantment(num);
			}
		}
	}

	public int GetEnchantmentDamage(Tyche tyche, EnchantmentDmgTriggerId dmgTriggerId)
	{
		List<Tuple<Enchantment, int>> enchantmentDamages = GetEnchantmentDamages(tyche, dmgTriggerId);
		int num = 0;
		for (int i = 0; i < enchantmentDamages.Count; i++)
		{
			num += enchantmentDamages[i].Item2;
		}
		return num;
	}

	public List<Tuple<Enchantment, int>> GetEnchantmentDamages(Tyche tyche, EnchantmentDmgTriggerId dmgTriggerId)
	{
		List<Tuple<Enchantment, int>> list = new List<Tuple<Enchantment, int>>();
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (Enchantments[i].Data.EnchantmentDmgTriggerId != dmgTriggerId || (dmgTriggerId == EnchantmentDmgTriggerId.ON_APPLY && Enchantments[i].damageApplied) || (!Enchantments[i].Data.Indestructible && (enchantTypeImmunities.IndexOf(Enchantments[i].Data.EnchantmentTypeId, EnchantmentTypeIdComparer.Instance) != -1 || enchantTypeToBeRemoved.IndexOf(Enchantments[i].Data.EnchantmentTypeId, EnchantmentTypeIdComparer.Instance) != -1 || enchantToBeRemoved.IndexOf(Enchantments[i].Id, EnchantmentIdComparer.Instance) != -1)))
			{
				continue;
			}
			bool flag = false;
			Enchantments[i].damageApplied = true;
			if (Enchantments[i].Data.AttributeIdDmgResistRoll != 0)
			{
				int num = GetAttribute(Enchantments[i].Data.AttributeIdDmgResistRoll);
				if (Enchantments[i].Provider != null)
				{
					num += Enchantments[i].Provider.GetAttributeModifier(Enchantments[i].Data.AttributeIdDmgResistRoll);
				}
				flag = Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, num, Enchantments[i].Data.AttributeIdDmgResistRoll, reverse: false, apply: false);
			}
			int item = 0;
			if (!flag)
			{
				item = tyche.Rand(Enchantments[i].Data.DamageMin, Enchantments[i].Data.DamageMax);
			}
			list.Add(new Tuple<Enchantment, int>(Enchantments[i], item));
		}
		return list;
	}

	public void UpdateEnchantmentsDuration(Unit currentUnit, bool updateAttributes = true)
	{
		bool flag = false;
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].UpdateDuration(currentUnit))
			{
				RemoveEnchantment(num);
				flag = true;
			}
		}
		for (int i = 0; i < Items.Count; i++)
		{
			flag |= Items[i].UpdateEnchantmentsDuration(currentUnit);
		}
		if (flag && updateAttributes)
		{
			UpdateAttributes();
		}
	}

	public void UpdateEnchantments()
	{
		bool flag = false;
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].UpdateStatus(Status))
			{
				RemoveEnchantment(num);
				flag = true;
			}
		}
		for (int i = 0; i < Items.Count; i++)
		{
			flag |= Items[i].UpdateEnchantments(Status);
		}
		if (flag)
		{
			UpdateAttributes();
		}
	}

	public void UpdateValidNextActionEnchantments()
	{
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].UpdateValidNextAction())
			{
				RemoveEnchantment(num);
			}
		}
	}

	public void UpdateEnchantmentsFx()
	{
		needFxRefresh = false;
		List<EnchantmentId> list = new List<EnchantmentId>();
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (Enchantments[i].HasFx && Enchantments[i].fxSpawned)
			{
				list.Add(Enchantments[i].Id);
			}
		}
		for (int j = 0; j < Enchantments.Count; j++)
		{
			if (Enchantments[j].HasFx && !Enchantments[j].fxSpawned && list.IndexOf(Enchantments[j].Id, EnchantmentIdComparer.Instance) == -1)
			{
				list.Add(Enchantments[j].Id);
				Enchantments[j].SpawnFx(this);
			}
		}
	}

	public void CleanEnchantments()
	{
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (!Enchantments[num].Innate && !Enchantments[num].Data.KeepOnDeath)
			{
				RemoveEnchantment(num);
			}
		}
	}

	public void ApplyTurnStartEnchantments()
	{
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			EnchantmentId enchantmentIdOnTurnStart = Enchantments[num].EnchantmentIdOnTurnStart;
			if (enchantmentIdOnTurnStart != 0)
			{
				AddEnchantment(enchantmentIdOnTurnStart, this, original: false);
			}
		}
	}

	private void ApplyItem(Item item)
	{
		for (int i = 0; i < item.AttributeModifiers.Count; i++)
		{
			ItemAttributeData itemAttributeData = item.AttributeModifiers[i];
			if ((itemAttributeData.UnitActionIdTrigger == UnitActionId.NONE && itemAttributeData.SkillIdTrigger == SkillId.NONE) || (ActiveSkill != null && ActiveSkill.UnitActionId != 0 && ActiveSkill.UnitActionId == itemAttributeData.UnitActionIdTrigger) || (ActiveSkill != null && ActiveSkill.Id != 0 && ActiveSkill.Id == itemAttributeData.SkillIdTrigger))
			{
				AddToAttribute(itemAttributeData.AttributeId, itemAttributeData.Modifier);
				AddAttributeModifier(AttributeMod.Type.ITEM, itemAttributeData.AttributeId, PandoraSingleton<LocalizationManager>.Instance.GetStringById(item.LabelName), itemAttributeData.Modifier);
			}
		}
		for (int j = 0; j < item.Enchantments.Count; j++)
		{
			ApplyEnchantment(item.Enchantments[j]);
		}
		if (item.RuneMark != null)
		{
			ApplyRuneMark(item.RuneMark);
		}
	}

	public ItemId GetItemId(UnitSlotId slot)
	{
		return Items[(int)slot].Id;
	}

	public void SetItems()
	{
		if ((!PandoraSingleton<MissionStartData>.Exists() || !PandoraSingleton<MissionStartData>.Instance.isReload) && UnitSave.campaignId != 0)
		{
			SetCampaignItems();
			return;
		}
		bool flag = true;
		for (int i = 0; i < UnitSave.items.Count; i++)
		{
			if (UnitSave.items[i] != null && UnitSave.items[i].id != 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			InitBaseEquipment();
		}
		SetSaveItems();
	}

	private void InitBaseEquipment()
	{
		for (int i = 0; i < 13; i++)
		{
			List<UnitBaseItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitBaseItemData>(new string[2]
			{
				"fk_unit_id",
				"fk_unit_slot_id"
			}, new string[2]
			{
				((int)Id).ToConstantString(),
				i.ToConstantString()
			});
			if (list != null && list.Count > 0)
			{
				UnitBaseItemData unitBaseItemData = list[0];
				MutationId mutationId = GetMutationId((UnitSlotId)i);
				UnitSave.items[(int)unitBaseItemData.UnitSlotId] = new ItemSave((mutationId != 0) ? unitBaseItemData.ItemIdMutation : unitBaseItemData.ItemId);
			}
		}
	}

	private void SetSaveItems()
	{
		for (int i = 0; i < UnitSave.items.Count; i++)
		{
			if (i < Items.Count && UnitSave.items[i] != null)
			{
				Items[i] = new Item(UnitSave.items[i]);
			}
		}
		if (CampaignData != null)
		{
			return;
		}
		for (int j = 0; j < 6; j++)
		{
			if (UnitSave.items[j] == null)
			{
				EquipItem((UnitSlotId)j, ItemId.NONE);
			}
			else
			{
				EquipItem((UnitSlotId)j, UnitSave.items[j]);
			}
		}
	}

	private void SetCampaignItems()
	{
		List<CampaignUnitJoinItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<CampaignUnitJoinItemData>("fk_campaign_unit_id", ((int)CampaignData.Id).ToConstantString());
		for (int i = 0; i < list.Count; i++)
		{
			CampaignUnitJoinItemData campaignUnitJoinItemData = list[i];
			Items[(int)campaignUnitJoinItemData.UnitSlotId] = new Item(campaignUnitJoinItemData.ItemId, campaignUnitJoinItemData.ItemQualityId);
			Items[(int)campaignUnitJoinItemData.UnitSlotId].owner = this;
			if (campaignUnitJoinItemData.RuneMarkId != 0)
			{
				Items[(int)campaignUnitJoinItemData.UnitSlotId].AddRuneMark(campaignUnitJoinItemData.RuneMarkId, campaignUnitJoinItemData.RuneMarkQualityId, AllegianceId);
			}
			UnitSave.items[(int)campaignUnitJoinItemData.UnitSlotId] = Items[(int)campaignUnitJoinItemData.UnitSlotId].Save;
		}
	}

	public bool GetEmptyItemSlot(out UnitSlotId slotId, Item item)
	{
		slotId = UnitSlotId.NB_SLOTS;
		int num = 6;
		while (num < Items.Count)
		{
			if (GetItemId((UnitSlotId)num) == ItemId.NONE || (item.IsStackable && GetItemId((UnitSlotId)num) == item.Id))
			{
				slotId = (UnitSlotId)num;
				return true;
			}
			num++;
			slotId++;
		}
		slotId = UnitSlotId.NB_SLOTS;
		return false;
	}

	public void CacheBackpackSize()
	{
		cachedBackpackCapacity = realBackPackCapacity;
	}

	public int GetNumUsedItemSlot()
	{
		return BackpackCapacity - GetNumEmptyItemSlot();
	}

	public int GetNumEmptyItemSlot()
	{
		int num = 0;
		for (int i = 6; i < Items.Count; i++)
		{
			if (Items[i].Id == ItemId.NONE)
			{
				num++;
			}
		}
		return num;
	}

	private void ReequipWeapons(List<Item> removedItems)
	{
		for (int i = 2; i <= 5; i++)
		{
			removedItems.AddRange(EquipItem((UnitSlotId)i, Items[i]));
		}
	}

	public List<Item> EquipItem(UnitSlotId slot, ItemId itemId)
	{
		return EquipItem(slot, new ItemSave(itemId));
	}

	public List<Item> EquipItem(UnitSlotId slot, ItemSave itemSave)
	{
		return EquipItem(slot, new Item(itemSave));
	}

	public List<Item> EquipItem(UnitSlotId slot, Item item, bool sortItems = true)
	{
		List<Item> previousItems = new List<Item>();
		MutationId mutationId = GetMutationId(slot);
		item = SetItem(slot, item, mutationId, ref previousItems);
		if (item.Id != 0 && slot < UnitSlotId.ITEM_1)
		{
			bool flag = false;
			if (slot == UnitSlotId.SET1_MAINHAND || slot == UnitSlotId.SET2_MAINHAND)
			{
				for (int i = 0; i < item.Enchantments.Count; i++)
				{
					if (item.Enchantments[i].Id == EnchantmentId.ITEM_UNWIELDY)
					{
						flag = true;
						break;
					}
				}
			}
			bool flag2 = (slot == UnitSlotId.SET1_MAINHAND || slot == UnitSlotId.SET2_MAINHAND) && Items[(int)(slot + 1)].Id == ItemId.NONE;
			if (item.IsPaired || flag2 || item.IsTwoHanded || (flag && Items[(int)(slot + 1)] != null && Items[(int)(slot + 1)].Id != 0 && Items[(int)(slot + 1)].TypeData.Id != ItemTypeId.SHIELD))
			{
				SetItem(slot + 1, new Item(ItemId.NONE), mutationId, ref previousItems);
			}
			if (flag && GetMutationId(slot + 1) != 0)
			{
				SetItem(slot, new Item(ItemId.NONE), mutationId, ref previousItems);
			}
			if (item.IsLockSlot && (slot == UnitSlotId.SET1_MAINHAND || slot == UnitSlotId.SET1_OFFHAND))
			{
				List<Item> previousItems2 = new List<Item>(0);
				ItemSave itemSave = new ItemSave(ItemId.NONE);
				Thoth.Copy(item.Save, itemSave);
				SetItem(slot + 2, new Item(itemSave), mutationId, ref previousItems2);
			}
		}
		else if (item.Id == ItemId.NONE && slot == UnitSlotId.SET2_MAINHAND)
		{
			SetItem(UnitSlotId.SET2_OFFHAND, new Item(ItemId.NONE), GetMutationId(UnitSlotId.SET2_OFFHAND), ref previousItems);
		}
		if (sortItems && slot >= UnitSlotId.ITEM_1)
		{
			Item.SortEmptyItems(items, 6);
			for (int j = 6; j < items.Count; j++)
			{
				UnitSave.items[j] = items[j].Save;
			}
		}
		return previousItems;
	}

	public void CheckItemsAchievments()
	{
		bool flag = true;
		bool flag2 = true;
		ItemQualityId itemQualityId = ItemQualityId.NONE;
		RuneMarkQualityId runeMarkQualityId = RuneMarkQualityId.NONE;
		for (int i = 0; i < 6; i++)
		{
			if ((i != 3 && i != 5) || (!Items[i - 1].IsPaired && !Items[i - 1].IsTwoHanded))
			{
				if (Items[i].Id == ItemId.NONE || (itemQualityId != 0 && Items[i].QualityData.Id != itemQualityId) || Items[i].Backup)
				{
					flag = false;
				}
				else if (flag)
				{
					itemQualityId = Items[i].QualityData.Id;
				}
				if (Items[i].Id == ItemId.NONE || Items[i].RuneMark == null || (runeMarkQualityId != 0 && Items[i].RuneMark.QualityData.Id != runeMarkQualityId))
				{
					flag2 = false;
				}
				else if (flag2)
				{
					runeMarkQualityId = Items[i].RuneMark.QualityData.Id;
				}
			}
		}
		if (flag)
		{
			Hephaestus.TrophyId achievement = Hephaestus.TrophyId.NORMAL_EQUIP;
			switch (itemQualityId)
			{
			case ItemQualityId.GOOD:
				achievement = Hephaestus.TrophyId.GOOD_EQUIP;
				break;
			case ItemQualityId.BEST:
				achievement = Hephaestus.TrophyId.BEST_EQUIP;
				break;
			}
			PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(achievement);
		}
		if (flag2)
		{
			PandoraSingleton<Hephaestus>.Instance.UnlockAchievement((runeMarkQualityId != RuneMarkQualityId.REGULAR) ? Hephaestus.TrophyId.ENCHANT_EQUIP_2 : Hephaestus.TrophyId.ENCHANT_EQUIP_1);
		}
	}

	private Item SetItem(UnitSlotId slot, Item item, MutationId mutationId, ref List<Item> previousItems)
	{
		if (Items[(int)slot].IsUndroppable)
		{
			previousItems.Add(item);
			return new Item(ItemId.NONE);
		}
		UnitSlotData unitSlotData = PandoraSingleton<DataFactory>.Instance.InitData<UnitSlotData>((int)slot);
		bool flag = IsBoneBlocked(unitSlotData.BoneId);
		if (slot < UnitSlotId.ITEM_1)
		{
			if (flag || IsItemTypeBlocked(item.TypeData.Id))
			{
				if (item.Id != 0)
				{
					previousItems.Add(item);
				}
				item = new Item(ItemId.NONE);
			}
			List<ItemUnitData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemUnitData>(new string[2]
			{
				"fk_unit_id",
				"mutation"
			}, new string[2]
			{
				((int)Id).ToConstantString(),
				(mutationId == MutationId.NONE) ? "0" : "1"
			});
			if (list.Find((ItemUnitData x) => x.ItemId == item.Id) == null)
			{
				item = new Item(ItemId.NONE);
			}
			bool flag2 = false;
			if (item.IsPaired || item.IsTwoHanded)
			{
				UnitSlotData unitSlotData2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitSlotData>((int)slot);
				flag2 = blockedBones.ContainsKey(unitSlotData2.BoneId);
				if (item.Id != 0 && (slot == UnitSlotId.SET1_MAINHAND || slot == UnitSlotId.SET2_MAINHAND) && (GetMutationId(slot + 1) != 0 || GetInjury(slot + 1) != 0))
				{
					previousItems.Add(item);
					item = new Item(ItemId.NONE);
				}
			}
			if (item.Id == ItemId.NONE && !flag && ((slot != UnitSlotId.SET1_OFFHAND && slot != UnitSlotId.SET2_OFFHAND) || ((slot == UnitSlotId.SET1_OFFHAND || slot == UnitSlotId.SET2_OFFHAND) && !Items[(int)(slot - 1)].IsPaired && !Items[(int)(slot - 1)].IsTwoHanded && Items[(int)(slot - 1)].Id != 0) || flag2))
			{
				List<UnitDefaultItemData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitDefaultItemData>(new string[2]
				{
					"fk_unit_id",
					"fk_unit_slot_id"
				}, new string[2]
				{
					((int)Id).ToConstantString(),
					((int)slot).ToConstantString()
				});
				if (list2.Count != 0)
				{
					item = new Item((mutationId == MutationId.NONE) ? list2[0].ItemId : list2[0].ItemIdMutation);
				}
			}
		}
		if (item.ConsumableData != null && item.ConsumableData.OutOfCombat && !PandoraSingleton<MissionManager>.Exists())
		{
			UnitSave.consumableSkills.Add(item.ConsumableData.SkillId);
			AddSkill(item.ConsumableData.SkillId, updateAttributes: false);
			return item;
		}
		Item item2 = Items[(int)slot];
		if (!item.IsTrophy)
		{
			ItemId id = item.Id;
			if (id == ItemId.WYRDSTONE_SHARD || id == ItemId.WYRDSTONE_CLUSTER || id == ItemId.WYRDSTONE_FRAGMENT)
			{
				if (item.owner == null || item.owner.Status == UnitStateId.OUT_OF_ACTION || item.owner.warbandIdx != warbandIdx)
				{
					item.owner = this;
				}
			}
			else
			{
				item.owner = this;
			}
		}
		item.SetModifiers(mutationId);
		if (item2 != null && item2.IsStackable && item2.Id == item.Id)
		{
			item.Save.amount += item2.Save.amount;
		}
		else if (item2 != null && item2.Id != 0 && !item2.Backup)
		{
			previousItems.Add(item2);
		}
		UnitSave.items[(int)slot] = item.Save;
		Items[(int)slot] = item;
		SetBodyPartItemLock(item2, reverse: true);
		SetBodyPartsSlot(slot);
		return item;
	}

	public List<Item> UnequipAllItems()
	{
		List<Item> list = new List<Item>();
		for (int i = 0; i < Items.Count; i++)
		{
			UnitSlotId unitSlotId = (UnitSlotId)i;
			Item item = Items[i];
			if (item != null && ((unitSlotId == UnitSlotId.SET1_OFFHAND && !Items[i - 1].IsPaired) || unitSlotId != UnitSlotId.SET1_OFFHAND) && ((unitSlotId == UnitSlotId.SET2_MAINHAND && !Items[i - 2].IsLockSlot) || unitSlotId != UnitSlotId.SET2_MAINHAND) && ((unitSlotId == UnitSlotId.SET2_MAINHAND && GetMutationId(unitSlotId) == MutationId.NONE) || unitSlotId != UnitSlotId.SET2_MAINHAND) && ((unitSlotId == UnitSlotId.SET2_OFFHAND && GetMutationId(unitSlotId) == MutationId.NONE) || unitSlotId != UnitSlotId.SET2_OFFHAND) && ((unitSlotId == UnitSlotId.SET2_OFFHAND && !Items[i - 2].IsLockSlot) || unitSlotId != UnitSlotId.SET2_OFFHAND) && ((unitSlotId == UnitSlotId.SET2_OFFHAND && !Items[i - 1].IsPaired) || unitSlotId != UnitSlotId.SET2_OFFHAND))
			{
				list.Add(item);
			}
		}
		for (int j = 0; j < 13; j++)
		{
			UnitSave.items[j] = null;
		}
		SetItems();
		return list;
	}

	public bool HasItem(Item item)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (Items[i].IsSame(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasItem(ItemId id, ItemQualityId qualityId = ItemQualityId.NONE)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (Items[i].Id == id && (qualityId == ItemQualityId.NONE || Items[i].QualityData.Id == qualityId))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasItem(ItemTypeId typeId)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (Items[i].TypeData.Id == typeId)
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshActiveItems()
	{
		ActiveItems.Clear();
		for (int i = 0; i < 6; i++)
		{
			Item item = Items[i];
			if (item.Id != 0 && i != (int)InactiveWeaponSlot && i != (int)(InactiveWeaponSlot + 1) && ((item.IsPaired && i != 3 && i != 5) || !item.IsPaired))
			{
				ActiveItems.Add(item);
			}
		}
	}

	public bool HasItemActive(ItemId id)
	{
		for (int i = 0; i < ActiveItems.Count; i++)
		{
			if (ActiveItems[i].Id == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInventoryFull()
	{
		bool result = true;
		for (int i = 6; i < Items.Count; i++)
		{
			if (Items[i].Id == ItemId.NONE)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool IsInventoryEmpty()
	{
		bool result = true;
		for (int i = 6; i < Items.Count; i++)
		{
			if (Items[i].Id != 0)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public void ConsumeItem(ItemId itemId)
	{
		int num = 0;
		while (true)
		{
			if (num < Items.Count)
			{
				if (Items[num].Id == itemId)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		EquipItem((UnitSlotId)num, ItemId.NONE);
	}

	public void RemoveItem(Item item)
	{
		int num = 0;
		while (true)
		{
			if (num < Items.Count)
			{
				if (Items[num].Id == item.Id && Items[num].QualityData.Id == item.QualityData.Id && ((Items[num].RuneMark == null && item.RuneMark == null) || (Items[num].RuneMark != null && item.RuneMark != null && Items[num].RuneMark.Data.Id == item.RuneMark.Data.Id && Items[num].RuneMark.QualityData.Id == item.RuneMark.QualityData.Id)))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		EquipItem((UnitSlotId)num, ItemId.NONE);
	}

	public bool WeaponSlotsLocked()
	{
		return (Items[2].IsLockSlot && Items[3].IsLockSlot) || (Items[2].IsLockSlot && Items[2].IsTwoHanded);
	}

	public bool BothArmsMutated()
	{
		return GetMutationId(UnitSlotId.SET1_MAINHAND) != 0 && GetMutationId(UnitSlotId.SET1_OFFHAND) != MutationId.NONE;
	}

	public bool HasMutatedArm()
	{
		return GetMutationId(UnitSlotId.SET1_MAINHAND) != 0 || GetMutationId(UnitSlotId.SET1_OFFHAND) != MutationId.NONE;
	}

	public bool CanSwitchWeapon()
	{
		return Items[(int)InactiveWeaponSlot].Id != 0 && !WeaponSlotsLocked();
	}

	public AttributeModList GetSpellDamageModifier(SkillId skillId, Unit target, SpellTypeId spellType, bool bypassArmor = false)
	{
		weaponDamageModifiers.Clear();
		string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_damage");
		if (DamageBonusSpell != 0)
		{
			weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_SPELL), stringById);
		}
		switch (spellType)
		{
		case SpellTypeId.ARCANE:
			if (DamageBonusArcMagPerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_ARC_MAG_PERC), stringById, isPercent: true);
			}
			break;
		case SpellTypeId.DIVINE:
			if (DamageBonusDivMagPerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_DIV_MAG_PERC), stringById, isPercent: true);
			}
			break;
		}
		if (target != null && target.damagePercModifiers.ContainsKey(skillId))
		{
			weaponDamageModifiers.Add(new AttributeMod(PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_damage_bonus_spell_perc", target.damagePercModifiers[skillId].ToString())));
		}
		if (bypassArmor)
		{
			weaponDamageModifiers.Add(new AttributeMod(PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_ignore_armor")));
		}
		else
		{
			if (target != null && target.ArmorAbsorption != 0)
			{
				weaponDamageModifiers.AddRange(target.attributeModifiers.GetOrNull(AttributeId.ARMOR_ABSORPTION), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_armor_absorption"), isPercent: false, isEnemyMod: true, negate: true);
			}
			if (target != null && target.ArmorAbsorptionPerc != 0)
			{
				weaponDamageModifiers.AddRange(target.attributeModifiers.GetOrNull(AttributeId.ARMOR_ABSORPTION_PERC), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_armor_absorption_perc"), isPercent: true, isEnemyMod: true, negate: true);
			}
		}
		return weaponDamageModifiers;
	}

	public int ApplySpellDamageModifier(SkillId skillId, int damage, Unit target, SpellTypeId spellType, bool byPassArmor)
	{
		int num = 0;
		float num2 = 100f;
		num += DamageBonusSpell;
		switch (spellType)
		{
		case SpellTypeId.ARCANE:
			num2 += (float)DamageBonusArcMagPerc;
			break;
		case SpellTypeId.DIVINE:
			num2 += (float)DamageBonusDivMagPerc;
			break;
		}
		if (target != null && target.damagePercModifiers.ContainsKey(skillId))
		{
			num2 += (float)target.damagePercModifiers[skillId];
		}
		int num3 = PandoraUtils.Round((float)(damage + num) * (num2 / 100f));
		if (target != null && !byPassArmor)
		{
			float num4 = Mathf.Max(target.ArmorAbsorptionPerc, 0);
			num3 -= Mathf.Max(target.ArmorAbsorption, 0);
			num3 -= PandoraUtils.Round((float)num3 * num4 / 100f);
		}
		return (num3 > 0) ? num3 : 0;
	}

	public AttributeModList GetWeaponDamageModifier(Unit target, bool bypassArmor = false, bool isCharging = false)
	{
		weaponDamageModifiers.Clear();
		bool flag = HasRange();
		weaponDamageModifiers.Add(new AttributeMod(AttributeMod.Type.BASE, PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_base_weapon_damage"), DamageMin, DamageMax));
		string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_damage");
		if (DamageBonus != 0)
		{
			weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS), stringById);
		}
		string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_global_damage");
		if (flag)
		{
			if (DamageBonusRange != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_RANGE), stringById);
			}
			if (DamageBonusRangePerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_RANGE_PERC), stringById, isPercent: true);
			}
			if (GlobalRangeDamagePerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.GLOBAL_RANGE_DAMAGE_PERC), stringById2, isPercent: true);
			}
		}
		else
		{
			if (DamageBonusMelee != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_MELEE), stringById);
			}
			if (DamageBonusMeleePerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_MELEE_PERC), stringById, isPercent: true);
			}
			if (isCharging && DamageBonusChargePerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.DAMAGE_BONUS_CHARGE_PERC), stringById, isPercent: true);
			}
			if (GlobalMeleeDamagePerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.GLOBAL_MELEE_DAMAGE_PERC), stringById2, isPercent: true);
			}
		}
		if (target != null)
		{
			if (target.AllegianceId == AllegianceId.DESTRUCTION)
			{
				if (DamageHoly != 0)
				{
					weaponDamageModifiers.Add(new AttributeMod(AttributeMod.Type.NONE, AttributeId.DAMAGE_HOLY.ToLowerString(), DamageHoly, stringById));
				}
				if (DamageBonusHolyPerc != 0)
				{
					weaponDamageModifiers.Add(new AttributeMod(AttributeMod.Type.NONE, AttributeId.DAMAGE_BONUS_HOLY_PERC.ToLowerString(), DamageBonusHolyPerc, stringById));
				}
			}
			else
			{
				if (DamageUnholy != 0)
				{
					weaponDamageModifiers.Add(new AttributeMod(AttributeMod.Type.NONE, AttributeId.DAMAGE_UNHOLY.ToLowerString(), DamageUnholy, stringById));
				}
				if (DamageBonusUnholyPerc != 0)
				{
					weaponDamageModifiers.Add(new AttributeMod(AttributeMod.Type.NONE, AttributeId.DAMAGE_BONUS_UNHOLY_PERC.ToLowerString(), DamageBonusUnholyPerc, stringById));
				}
			}
		}
		if (bypassArmor)
		{
			weaponDamageModifiers.Add(new AttributeMod(PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_ignore_armor")));
		}
		else
		{
			if (BypassArmor != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.BYPASS_ARMOR), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_bypass_armor"));
			}
			if (ByPassArmorPerc != 0)
			{
				weaponDamageModifiers.AddRange(attributeModifiers.GetOrNull(AttributeId.BYPASS_ARMOR_PERC), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_bypass_armor_perc"), isPercent: true);
			}
		}
		if (target != null && !bypassArmor)
		{
			if (target.ArmorAbsorption != 0)
			{
				weaponDamageModifiers.AddRange(target.attributeModifiers.GetOrNull(AttributeId.ARMOR_ABSORPTION), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_armor_absorption"), isPercent: false, isEnemyMod: true, negate: true);
			}
			if (target.ArmorAbsorptionPerc != 0)
			{
				weaponDamageModifiers.AddRange(target.attributeModifiers.GetOrNull(AttributeId.ARMOR_ABSORPTION_PERC), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_armor_absorption_perc"), isPercent: true, isEnemyMod: true, negate: true);
			}
		}
		return weaponDamageModifiers;
	}

	public int GetWeaponDamageMin(Unit target, bool critical = false, bool byPassArmor = false, bool charging = false)
	{
		return ApplyWeaponDamageModifier(DamageMin, target, critical, byPassArmor, charging);
	}

	public int GetWeaponDamageMax(Unit target, bool critical = false, bool byPassArmor = false, bool charging = false)
	{
		return ApplyWeaponDamageModifier(DamageMax, target, critical, byPassArmor, charging);
	}

	private int ApplyWeaponDamageModifier(int damage, Unit target, bool critical, bool byPassArmor, bool charging)
	{
		int num = 0;
		float num2 = 100f;
		bool flag = HasRange();
		num += DamageBonus;
		num += ((!flag) ? DamageBonusMelee : DamageBonusRange);
		num += (critical ? DamageCriticalBonus : 0);
		num2 += (float)((!flag) ? DamageBonusMeleePerc : DamageBonusRangePerc);
		num2 += (float)(charging ? DamageBonusChargePerc : 0);
		num2 += (float)(critical ? DamageCriticalBonusPerc : 0);
		if (target != null)
		{
			num += ((target.AllegianceId != AllegianceId.DESTRUCTION) ? DamageUnholy : DamageHoly);
			num2 += (float)((target.AllegianceId != AllegianceId.DESTRUCTION) ? DamageBonusUnholyPerc : DamageBonusHolyPerc);
		}
		int num3 = PandoraUtils.Round((float)(damage + num) * (num2 / 100f));
		float num4 = 100f + (float)((!flag) ? GlobalMeleeDamagePerc : GlobalRangeDamagePerc);
		num3 = PandoraUtils.Round((float)num3 * (num4 / 100f));
		if (target != null && !critical && !byPassArmor)
		{
			float num5 = Mathf.Max(target.ArmorAbsorptionPerc - ByPassArmorPerc, 0);
			num3 -= Mathf.Max(target.ArmorAbsorption - BypassArmor, 0);
			num3 -= PandoraUtils.Round((float)num3 * num5 / 100f);
		}
		return (num3 > 0) ? num3 : 0;
	}

	public bool HasRange()
	{
		if (Items[(int)ActiveWeaponSlot] != null && Items[(int)ActiveWeaponSlot].TypeData != null)
		{
			return Items[(int)ActiveWeaponSlot].TypeData.IsRange;
		}
		return false;
	}

	private void ApplyRuneMark(RuneMark runeMark)
	{
		for (int i = 0; i < runeMark.AttributeModifiers.Count; i++)
		{
			RuneMarkAttributeData runeMarkAttributeData = runeMark.AttributeModifiers[i];
			if ((runeMarkAttributeData.UnitActionId == UnitActionId.NONE && runeMarkAttributeData.SkillId == SkillId.NONE) || (ActiveSkill != null && ActiveSkill.UnitActionId != 0 && ActiveSkill.UnitActionId == runeMarkAttributeData.UnitActionId) || (ActiveSkill != null && ActiveSkill.Id != 0 && ActiveSkill.Id == runeMarkAttributeData.SkillId))
			{
				AddToAttribute(runeMarkAttributeData.AttributeId, runeMarkAttributeData.Modifier);
				AddAttributeModifier(AttributeMod.Type.ITEM, runeMarkAttributeData.AttributeId, PandoraSingleton<LocalizationManager>.Instance.GetStringById(runeMark.FullLabel), runeMarkAttributeData.Modifier);
			}
		}
		for (int j = 0; j < runeMark.Enchantments.Count; j++)
		{
			ApplyEnchantment(runeMark.Enchantments[j]);
		}
	}

	public void SetStatus(UnitStateId newStatus)
	{
		if (PandoraSingleton<MissionManager>.Exists())
		{
			if (newStatus == UnitStateId.STUNNED && Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, StunResistRoll, AttributeId.STUN_RESIST_ROLL))
			{
				return;
			}
			if (newStatus != Status)
			{
				if (newStatus == UnitStateId.OUT_OF_ACTION)
				{
					PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, CombatLogger.LogMessage.UNIT_OUT_OF_ACTION, PandoraSingleton<MissionManager>.Instance.GetUnitController(this).GetLogName(), GetAttribute(AttributeId.MORAL_IMPACT).ToConstantString());
					AddToAttribute(AttributeId.TOTAL_OOA, 1);
				}
				else
				{
					PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(this, CombatLogger.LogMessage.UNIT_STATUS, PandoraSingleton<MissionManager>.Instance.GetUnitController(this).GetLogName(), PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_state_" + newStatus));
					if (newStatus == UnitStateId.STUNNED)
					{
						AddEnchantment(EnchantmentId.FREE_STANCE_REMOVAL, this, original: false);
					}
					if (stunningUnit != null && newStatus == UnitStateId.STUNNED)
					{
						UnitController unitController = PandoraSingleton<MissionManager>.Instance.GetUnitController(this);
						UnitController unitController2 = PandoraSingleton<MissionManager>.Instance.GetUnitController(stunningUnit);
						if (unitController != null && unitController2 != null && unitController2.IsPlayed() && !unitController.IsPlayed())
						{
							PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.STUN_ENEMIES, 1);
						}
					}
				}
			}
		}
		Status = newStatus;
		stunningUnit = null;
	}

	public bool IsAvailable()
	{
		return Status == UnitStateId.NONE;
	}

	public bool IsSkillBlocked(SkillData data, out EnchantmentId enchantmentId)
	{
		enchantmentId = EnchantmentId.NONE;
		if (blockedActions.TryGetValue(data.UnitActionId, out enchantmentId))
		{
			return true;
		}
		if (blockedSkills.TryGetValue(data.Id, out enchantmentId))
		{
			return true;
		}
		return false;
	}

	public bool IsUnitActionBlocked(UnitActionId actionId)
	{
		return blockedActions.ContainsKey(actionId);
	}

	public bool IsBoneBlocked(BoneId boneId)
	{
		EnchantmentId enchantmentId;
		return IsBoneBlocked(boneId, out enchantmentId);
	}

	public bool IsBoneBlocked(BoneId bone, out EnchantmentId enchantmentId)
	{
		enchantmentId = EnchantmentId.NONE;
		if (blockedBones.TryGetValue(bone, out enchantmentId))
		{
			return true;
		}
		return false;
	}

	public bool IsItemTypeBlocked(ItemTypeId typeId)
	{
		EnchantmentId enchantmentId;
		return IsItemTypeBlocked(typeId, out enchantmentId);
	}

	public bool IsItemTypeBlocked(ItemTypeId typeId, out EnchantmentId enchantmentId)
	{
		enchantmentId = EnchantmentId.NONE;
		if (blockedItemTypes.TryGetValue(typeId, out enchantmentId))
		{
			return true;
		}
		return false;
	}

	public void ResetPoints()
	{
		SetAttribute(AttributeId.CURRENT_OFFENSE_POINTS, OffensePoints);
		SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, StrategyPoints);
		ResetTempPoints();
	}

	public bool HasEnoughPoints(int stratPoints, int offPoints)
	{
		return Mathf.Max(0, stratPoints + tempStrategyPoints) <= CurrentStrategyPoints && Mathf.Max(0, offPoints + tempOffensePoints) <= CurrentOffensePoints;
	}

	public void ResetTempPoints()
	{
		tempOffensePoints = 0;
		tempStrategyPoints = 0;
	}

	public int GetStratCostModifier(SkillId skillId, UnitActionId actionId, SpellTypeId spellTypeId)
	{
		int num = 0;
		if (skillCostModifiers.ContainsKey(skillId))
		{
			num += skillCostModifiers[skillId].strat;
		}
		if (actionCostModifiers.ContainsKey(actionId))
		{
			num += actionCostModifiers[actionId].strat;
		}
		if (spellTypeModifiers.ContainsKey(spellTypeId))
		{
			num += spellTypeModifiers[spellTypeId].strat;
		}
		return num;
	}

	public int GetOffCostModifier(SkillId skillId, UnitActionId actionId, SpellTypeId spellTypeId)
	{
		int num = 0;
		if (skillCostModifiers.ContainsKey(skillId))
		{
			num += skillCostModifiers[skillId].off;
		}
		if (actionCostModifiers.ContainsKey(actionId))
		{
			num += actionCostModifiers[actionId].off;
		}
		if (spellTypeModifiers.ContainsKey(spellTypeId))
		{
			num += spellTypeModifiers[spellTypeId].off;
		}
		return num;
	}

	public void RemovePoints(int stratPoints, int offPoints)
	{
		SetAttribute(AttributeId.CURRENT_OFFENSE_POINTS, Mathf.Min(CurrentOffensePoints - Mathf.Max(offPoints, 0), OffensePoints));
		SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, Mathf.Min(CurrentStrategyPoints - Mathf.Max(stratPoints, 0), StrategyPoints));
		ResetTempPoints();
		PandoraDebug.LogInfo("Strat Points : " + CurrentStrategyPoints, "CHARACTER");
		PandoraDebug.LogInfo("Off Points : " + CurrentOffensePoints, "CHARACTER");
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UPDATE_POINTS);
		if (CurrentOffensePoints < 0 || CurrentStrategyPoints < 0)
		{
			PandoraDebug.LogWarning("Removing points when not enough points available");
		}
	}

	public void RemoveTempPoints()
	{
		AddToAttribute(AttributeId.CURRENT_OFFENSE_POINTS, -tempOffensePoints);
		SetAttribute(AttributeId.CURRENT_OFFENSE_POINTS, (CurrentOffensePoints >= 0) ? CurrentOffensePoints : 0);
		AddToAttribute(AttributeId.CURRENT_STRATEGY_POINTS, -tempStrategyPoints);
		SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, (CurrentStrategyPoints >= 0) ? CurrentStrategyPoints : 0);
		ResetTempPoints();
	}

	public int GetUnitTypeRating()
	{
		return PandoraSingleton<DataFactory>.Instance.InitData<UnitTypeData>((int)GetUnitTypeId()).Rating;
	}

	public int GetRankRating()
	{
		return PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)UnitSave.rankId).Rating;
	}

	public int GetStatsRating()
	{
		int num = 0;
		for (int i = 0; i < attributeDataList.Count; i++)
		{
			AttributeData attributeData = attributeDataList[i];
			if (attributeData.Rating != 0)
			{
				int baseAttribute = GetBaseAttribute(attributeData.Id);
				num += baseAttribute * attributeData.Rating;
			}
		}
		return num;
	}

	public int GetSkillsRating()
	{
		int num = 0;
		for (int i = 0; i < ActiveSkills.Count; i++)
		{
			num += SkillHelper.GetRating(ActiveSkills[i]);
		}
		for (int j = 0; j < PassiveSkills.Count; j++)
		{
			num += SkillHelper.GetRating(PassiveSkills[j]);
		}
		for (int k = 0; k < Spells.Count; k++)
		{
			num += SkillHelper.GetRating(Spells[k]);
		}
		return num;
	}

	public int GetInjuriesRating()
	{
		int num = 0;
		for (int i = 0; i < Injuries.Count; i++)
		{
			num += Injuries[i].Data.Rating;
		}
		return num;
	}

	public int GetMutationsRating()
	{
		int num = 0;
		for (int i = 0; i < Mutations.Count; i++)
		{
			num += Mutations[i].Data.Rating;
		}
		return num;
	}

	public int GetEquipmentRating()
	{
		int num = 0;
		for (int i = 0; i < Items.Count; i++)
		{
			num += Items[i].GetRating();
		}
		return num;
	}

	public int GetRating()
	{
		int num = 0;
		num += GetUnitTypeRating();
		num += GetRankRating();
		num += GetStatsRating();
		num += GetSkillsRating();
		num += GetInjuriesRating();
		num += GetMutationsRating();
		return num + GetEquipmentRating();
	}

	public bool IsHero()
	{
		UnitTypeId unitTypeId = GetUnitTypeId();
		return unitTypeId == UnitTypeId.LEADER || unitTypeId == UnitTypeId.HERO_1 || unitTypeId == UnitTypeId.HERO_2 || unitTypeId == UnitTypeId.HERO_3;
	}

	public UnitTypeId GetUnitTypeId()
	{
		return GetUnitTypeId(UnitSave, Data.UnitTypeId);
	}

	public static UnitTypeId GetUnitTypeId(UnitSave save, UnitTypeId baseUnitTypeId)
	{
		return (save.overrideTypeId == UnitTypeId.NONE) ? baseUnitTypeId : save.overrideTypeId;
	}

	public int GetHireCost()
	{
		if (GetUnitTypeId() == UnitTypeId.DRAMATIS || GetUnitTypeId() == UnitTypeId.MONSTER || UnitSave.isFreeOutsider)
		{
			return 0;
		}
		int num = GetUnitCost().Hiring;
		if (UnitSave.isOutsider)
		{
			num += UnspentSkill * Constant.GetInt(ConstantId.HIRE_UNIT_COST_PER_SKILL_POINT);
			num += UnspentSpell * Constant.GetInt(ConstantId.HIRE_UNIT_COST_PER_SKILL_POINT);
			num += Injuries.Count * Constant.GetInt(ConstantId.HIRE_UNIT_COST_PER_INJURY);
			for (int i = 0; i < items.Count; i++)
			{
				num += items[i].PriceSold;
			}
		}
		return num;
	}

	public UnitCostData GetUnitCost()
	{
		List<UnitCostData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitCostData>(new string[2]
		{
			"fk_unit_type_id",
			"rank"
		}, new string[2]
		{
			((int)GetUnitTypeId()).ToConstantString(),
			Rank.ToConstantString()
		});
		return list[0];
	}

	public int NeedLevelUp()
	{
		return UnspentPhysical + UnspentMartial + UnspentMental + UnspentSkill + UnspentSpell;
	}

	public int GetBaseAttribute(AttributeId attributeId)
	{
		int num = 0;
		AttributeData attributeData = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>((int)attributeId);
		for (int i = 0; i < baseAttributesData.Count; i++)
		{
			if (baseAttributesData[i].AttributeId == attributeId)
			{
				num += baseAttributesData[i].BaseValue;
				break;
			}
		}
		for (int j = 0; j < PassiveSkills.Count; j++)
		{
			List<SkillAttributeData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillAttributeData>("fk_skill_id", ((int)PassiveSkills[j].Id).ToConstantString());
			for (int k = 0; k < list.Count; k++)
			{
				SkillAttributeData skillAttributeData = list[k];
				if (skillAttributeData.AttributeId == attributeId && skillAttributeData.UnitActionIdTrigger == UnitActionId.NONE && skillAttributeData.SkillIdTrigger == SkillId.NONE)
				{
					num += skillAttributeData.Modifier;
				}
			}
		}
		for (int l = 0; l < Injuries.Count; l++)
		{
			for (int m = 0; m < Injuries[l].AttributeModifiers.Count; m++)
			{
				InjuryJoinAttributeData injuryJoinAttributeData = Injuries[l].AttributeModifiers[m];
				if (injuryJoinAttributeData.AttributeId == attributeId)
				{
					num += injuryJoinAttributeData.Modifier;
				}
			}
		}
		for (int n = 0; n < ConsumableSkills.Count; n++)
		{
			List<SkillAttributeData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<SkillAttributeData>("fk_skill_id", ((int)ConsumableSkills[n].Id).ToConstantString());
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				SkillAttributeData skillAttributeData2 = list2[num2];
				if (skillAttributeData2.AttributeId == attributeId && skillAttributeData2.UnitActionIdTrigger == UnitActionId.NONE && skillAttributeData2.SkillIdTrigger == SkillId.NONE)
				{
					num += skillAttributeData2.Modifier;
				}
			}
		}
		num += GetSaveAttribute(attributeId);
		if (attributeData.AttributeIdMax != 0)
		{
			num = Mathf.Clamp(num, 1, GetBaseAttribute(attributeData.AttributeIdMax));
		}
		for (int num3 = 0; num3 < baseAttributesDataMax.Count; num3++)
		{
			UnitTypeAttributeData unitTypeAttributeData = baseAttributesDataMax[num3];
			if (attributeId == unitTypeAttributeData.AttributeId)
			{
				num = Mathf.Min(num, unitTypeAttributeData.Max);
			}
		}
		return num;
	}

	public int GetSaveAttribute(AttributeId attributeId)
	{
		int value = 0;
		UnitSave.attributes.TryGetValue(attributeId, out value);
		return value;
	}

	public bool CanLowerAttribute(AttributeId attributeId)
	{
		return GetTempAttribute(attributeId) > 0;
	}

	private int GetUnspentPoints(AttributeId attributeId)
	{
		for (int i = 0; i < PhysicalAttributeIds.Length; i++)
		{
			if (PhysicalAttributeIds[i] == attributeId)
			{
				return UnspentPhysical;
			}
		}
		for (int j = 0; j < MentalAttributeIds.Length; j++)
		{
			if (MentalAttributeIds[j] == attributeId)
			{
				return UnspentMental;
			}
		}
		for (int k = 0; k < MartialAttributeIds.Length; k++)
		{
			if (MartialAttributeIds[k] == attributeId)
			{
				return UnspentMartial;
			}
		}
		return 0;
	}

	public bool CanRaiseAttribute(AttributeId attributeId)
	{
		int num = GetBaseAttribute(attributeId) + GetTempAttribute(attributeId);
		if (!maxAttributes.TryGetValue((int)attributeId, out AttributeId value))
		{
			return false;
		}
		int baseAttribute = GetBaseAttribute(value);
		if (num >= baseAttribute)
		{
			return false;
		}
		return GetUnspentPoints(attributeId) > 0;
	}

	public bool CanRaiseAttributeFast(AttributeId attributeId, int[] baseAttributes, int[] maxAttributes, int unspent)
	{
		int num = 20;
		int num2 = 20;
		switch (attributeId)
		{
		case AttributeId.STRENGTH:
			num = baseAttributes[0];
			num2 = maxAttributes[0];
			break;
		case AttributeId.TOUGHNESS:
			num = baseAttributes[1];
			num2 = maxAttributes[1];
			break;
		case AttributeId.AGILITY:
			num = baseAttributes[2];
			num2 = maxAttributes[2];
			break;
		case AttributeId.LEADERSHIP:
			num = baseAttributes[3];
			num2 = maxAttributes[3];
			break;
		case AttributeId.INTELLIGENCE:
			num = baseAttributes[4];
			num2 = maxAttributes[4];
			break;
		case AttributeId.ALERTNESS:
			num = baseAttributes[5];
			num2 = maxAttributes[5];
			break;
		case AttributeId.WEAPON_SKILL:
			num = baseAttributes[6];
			num2 = maxAttributes[6];
			break;
		case AttributeId.BALLISTIC_SKILL:
			num = baseAttributes[7];
			num2 = maxAttributes[7];
			break;
		case AttributeId.ACCURACY:
			num = baseAttributes[8];
			num2 = maxAttributes[8];
			break;
		}
		int num3 = num + GetTempAttribute(attributeId);
		if (num3 >= num2)
		{
			return false;
		}
		return unspent > 0;
	}

	public void RaiseAttribute(AttributeId attributeId, bool updateAttributes = true)
	{
		AddToTempAttribute(attributeId, 1);
		if (updateAttributes)
		{
			UpdateAttributes();
		}
	}

	public void LowerAttribute(AttributeId attributeId)
	{
		AddToTempAttribute(attributeId, -1);
		UpdateAttributes();
	}

	public bool HasSkillOrSpell(SkillId skillId)
	{
		for (int i = 0; i < PassiveSkills.Count; i++)
		{
			if (PassiveSkills[i].Id == skillId)
			{
				return true;
			}
		}
		for (int i = 0; i < ConsumableSkills.Count; i++)
		{
			if (ConsumableSkills[i].Id == skillId)
			{
				return true;
			}
		}
		for (int i = 0; i < ActiveSkills.Count; i++)
		{
			if (ActiveSkills[i].Id == skillId)
			{
				return true;
			}
		}
		for (int i = 0; i < Spells.Count; i++)
		{
			if (Spells[i].Id == skillId)
			{
				return true;
			}
		}
		return false;
	}

	public void EndLearnSkill(bool updateAttributes = true)
	{
		SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)UnitSave.skillInTrainingId);
		UnitSave.skillInTrainingId = SkillId.NONE;
		UnitSave.trainingTime = 0;
		if (skillData.SpellTypeId != 0)
		{
			UnitSave.spells.Add(skillData.Id);
		}
		else if (skillData.Passive)
		{
			UnitSave.passiveSkills.Add(skillData.Id);
		}
		else
		{
			UnitSave.activeSkills.Add(skillData.Id);
		}
		AddSkill(skillData.Id);
		AddSkillLearnBonus(skillData.Id);
		CalculateProgressionPoints();
		if (updateAttributes)
		{
			UpdateAttributes();
		}
	}

	private void AddSkillLearnBonus(SkillId skillId)
	{
		List<SkillLearnBonusData> skillLearnBonus = SkillHelper.GetSkillLearnBonus(skillId);
		if (skillLearnBonus.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < skillLearnBonus.Count; i++)
		{
			if (skillLearnBonus[i].UnitTypeId != 0)
			{
				UnitSave.overrideTypeId = skillLearnBonus[i].UnitTypeId;
				RefreshDescription();
				SetMoralImpact();
			}
		}
	}

	public void StartLearningSkill(SkillData skillData, int currentDate, bool log = true)
	{
		UnitSave.skillInTrainingId = skillData.Id;
		UnitSave.trainingTime = skillData.Time;
		CalculateProgressionPoints();
		if (log)
		{
			Logger.AddHistory(currentDate + skillData.Time, EventLogger.LogEvent.SKILL, (int)skillData.Id);
		}
	}

	public bool HasEnoughPointsForSkill(SkillData skillData)
	{
		if (skillData.SkillTypeId == SkillTypeId.SPELL_ACTION)
		{
			return skillData.Points <= UnspentSpell;
		}
		return skillData.Points <= UnspentSkill;
	}

	public bool CanLearnSkill(SkillData skillData, out string reason)
	{
		reason = null;
		if (GetActiveStatus() != 0)
		{
			reason = "na_skill_" + GetActiveStatus().ToLowerString();
		}
		if (skillData.SkillTypeId == SkillTypeId.SPELL_ACTION)
		{
			if (skillData.Points > UnspentSpell)
			{
				reason = "na_skill_not_enough_spell_points";
			}
		}
		else if (skillData.Points > UnspentSkill)
		{
			reason = "na_skill_not_enough_skill_points";
		}
		if (!SkillHelper.IsMastery(skillData))
		{
			if (skillData.SkillTypeId == SkillTypeId.SPELL_ACTION)
			{
				if (Spells.Count == 5)
				{
					reason = "na_skill_spell_slot_full";
				}
			}
			else
			{
				if (skillData.Passive && PassiveSkills.Count == 5)
				{
					reason = "na_skill_passive_slot_full";
				}
				if (!skillData.Passive && ActiveSkills.Count == 5)
				{
					reason = "na_skill_active_slot_full";
				}
			}
		}
		if (SkillHelper.IsMastery(skillData) && GetUnitTypeId() == UnitTypeId.HENCHMEN)
		{
			reason = "na_skill_henchmen";
		}
		if (skillData.AttributeIdStat != 0 && GetBaseAttribute(skillData.AttributeIdStat) < skillData.StatValue)
		{
			reason = "na_skill_attribute";
		}
		return reason == null;
	}

	public bool CanLearnSkillFast(SkillData skillData, int[] baseAttributes)
	{
		if (GetUnitTypeId() == UnitTypeId.HENCHMEN && SkillHelper.IsMastery(skillData))
		{
			return false;
		}
		int num = 20;
		switch (skillData.AttributeIdStat)
		{
		case AttributeId.STRENGTH:
			num = baseAttributes[0];
			break;
		case AttributeId.TOUGHNESS:
			num = baseAttributes[1];
			break;
		case AttributeId.AGILITY:
			num = baseAttributes[2];
			break;
		case AttributeId.LEADERSHIP:
			num = baseAttributes[3];
			break;
		case AttributeId.INTELLIGENCE:
			num = baseAttributes[4];
			break;
		case AttributeId.ALERTNESS:
			num = baseAttributes[5];
			break;
		case AttributeId.WEAPON_SKILL:
			num = baseAttributes[6];
			break;
		case AttributeId.BALLISTIC_SKILL:
			num = baseAttributes[7];
			break;
		case AttributeId.ACCURACY:
			num = baseAttributes[8];
			break;
		}
		if (num < skillData.StatValue)
		{
			return false;
		}
		return true;
	}

	public bool IsMaxRank()
	{
		UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)UnitSave.rankId);
		if (unitRankData.Rank >= Constant.GetInt(ConstantId.MAX_UNIT_RANK))
		{
			return true;
		}
		return false;
	}

	public void AddXp(int xp, List<UnitJoinUnitRankData> advancements, List<Mutation> newMutations, List<Item> previousItems, int day, int maxRank = -1)
	{
		UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)UnitSave.rankId);
		bool flag = maxRank == -1;
		if (IsMaxRank())
		{
			return;
		}
		List<UnitRankProgressionData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankProgressionData>("fk_unit_type_id", ((int)GetUnitTypeId()).ToConstantString());
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			UnitRankProgressionData unitRankProgressionData = list[i];
			if (unitRankProgressionData.Rank == unitRankData.Rank)
			{
				num = unitRankProgressionData.Xp;
				break;
			}
		}
		int num2 = 0;
		for (int j = 0; j < ranksData.Count; j++)
		{
			if (ranksData[j].UnitRankId == UnitSave.rankId)
			{
				num2 = j;
				break;
			}
		}
		UnitSave.xp += xp;
		UnitSave.xp = Mathf.Max(UnitSave.xp, 0);
		while (UnitSave.xp >= num && unitRankData.Rank < Constant.GetInt(ConstantId.MAX_UNIT_RANK) && (maxRank == -1 || unitRankData.Rank < maxRank))
		{
			UnitSave.xp -= num;
			num2++;
			UnitJoinUnitRankData unitJoinUnitRankData = ranksData[num2];
			UnitSave.rankId = unitJoinUnitRankData.UnitRankId;
			if (unitJoinUnitRankData.EnchantmentId != 0)
			{
				Enchantments.Add(new Enchantment(unitJoinUnitRankData.EnchantmentId, null, this, orig: true, innate: true));
			}
			advancements.Add(unitJoinUnitRankData);
			if (unitJoinUnitRankData.Mutation)
			{
				Mutation item = AddRandomMutation(previousItems);
				newMutations.Add(item);
			}
			UnitRankData unitRankData2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)UnitSave.rankId);
			if (unitRankData2.Rank != unitRankData.Rank)
			{
				if (flag)
				{
					Logger.AddHistory(day, EventLogger.LogEvent.RANK_ACHIEVED, unitRankData2.Rank);
				}
				unitRankData = unitRankData2;
				SetAttribute(AttributeId.RANK, unitRankData.Rank);
				PandoraDebug.LogDebug("Rank is now = " + unitRankData.Rank);
				foreach (UnitRankProgressionData item2 in list)
				{
					if (item2.Rank == unitRankData.Rank)
					{
						num = item2.Xp;
						break;
					}
				}
			}
		}
		ResetBodyPart();
		CalculateProgressionPoints();
		UpdateAttributes();
		SetMoralImpact();
	}

	public int GetUpkeepOwned()
	{
		return UnitSave.upkeepOwned;
	}

	public int GetUpkeepMissedDays()
	{
		return UnitSave.upkeepMissedDays;
	}

	public int AddToUpkeepOwned(int money)
	{
		if (UnitSave.upkeepOwned > 0 && money > 0)
		{
			UnitSave.upkeepMissedDays++;
		}
		UnitSave.upkeepOwned += money;
		return GetUpkeepMissedDays();
	}

	public void PayUpkeepOwned()
	{
		UnitSave.upkeepOwned = 0;
		UnitSave.upkeepMissedDays = 0;
		Logger.RemoveLastHistory(EventLogger.LogEvent.LEFT);
	}

	public UnitActiveStatusId GetActiveStatus()
	{
		if (!UnitSave.injuryPaid && UnitSave.injuredTime > 0)
		{
			return UnitActiveStatusId.TREATMENT_NOT_PAID;
		}
		if (UnitSave.injuredTime > 0)
		{
			if (UnitSave.upkeepOwned > 0)
			{
				return UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID;
			}
			return UnitActiveStatusId.INJURED;
		}
		if (UnitSave.upkeepOwned > 0)
		{
			return UnitActiveStatusId.UPKEEP_NOT_PAID;
		}
		if (UnitSave.trainingTime > 0)
		{
			return UnitActiveStatusId.IN_TRAINING;
		}
		return UnitActiveStatusId.AVAILABLE;
	}

	public void ResetBodyPart()
	{
		foreach (BodyPart value2 in bodyParts.Values)
		{
			value2.DestroyRelatedGO();
		}
		bodyParts.Clear();
		for (int i = 0; i < availableBodyParts.Count; i++)
		{
			UnitSave.customParts.TryGetValue(availableBodyParts[i].Id, out KeyValuePair<int, int> value);
			bodyParts[availableBodyParts[i].Id] = new BodyPart(availableBodyParts[i], Id, WarData.Asset, Data.Asset, Data.AltAsset, UnitSave.skinColor, value.Value, value.Key);
		}
		for (int j = 0; j < Items.Count; j++)
		{
			SetBodyPartsSlot((UnitSlotId)j);
		}
		for (int k = 0; k < Mutations.Count; k++)
		{
			for (int l = 0; l < Mutations[k].RelatedBodyParts.Count; l++)
			{
				BodyPartId bodyPartId = Mutations[k].RelatedBodyParts[l].BodyPartId;
				if (bodyParts.ContainsKey(bodyPartId))
				{
					bodyParts[bodyPartId].SetMutation(Mutations[k].Data.Id);
					if (Mutations[k].GroupData.EmptyLinkedBodyPart)
					{
						EmptyLinkedBodyParts(bodyPartId, mutation: true);
					}
				}
			}
		}
		for (int m = 0; m < Injuries.Count; m++)
		{
			InjuryData data = Injuries[m].Data;
			if (data.BodyPartId != 0)
			{
				bodyParts[data.BodyPartId].SetInjury(data.Id);
				EmptyLinkedBodyParts(data.BodyPartId, mutation: false);
			}
		}
	}

	private void EmptyLinkedBodyParts(BodyPartId partId, bool mutation)
	{
		List<BodyPartUpdateData> list = PandoraSingleton<DataFactory>.Instance.InitData<BodyPartUpdateData>("fk_body_part_id", partId.ToIntString());
		for (int i = 0; i < list.Count; i++)
		{
			if (bodyParts.ContainsKey(list[i].BodyPartIdUpdated) && (!mutation || list[i].BodyPartIdUpdated != BodyPartId.GEAR_ARML))
			{
				bodyParts[list[i].BodyPartIdUpdated].SetEmpty(empty: true);
			}
		}
	}

	public void SetBodyPartsSlot(UnitSlotId slotId)
	{
		List<BodyPartData> list = PandoraSingleton<DataFactory>.Instance.InitData<BodyPartData>("fk_unit_slot_id", slotId.ToIntString());
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (bodyParts.ContainsKey(list[i].Id))
				{
					bodyParts[list[i].Id].SetRelatedItem(Items[(int)slotId]);
				}
			}
		}
		SetBodyPartItemLock(Items[(int)slotId]);
	}

	private void SetBodyPartItemLock(Item item, bool reverse = false)
	{
		if (item == null)
		{
			return;
		}
		List<ItemJoinBodyPartData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemJoinBodyPartData>("fk_item_id", item.Id.ToIntString());
		for (int i = 0; i < list.Count; i++)
		{
			if (!bodyParts.ContainsKey(list[i].BodyPartId))
			{
				continue;
			}
			bodyParts[list[i].BodyPartId].SetLocked((!reverse) ? list[i].Lock : (!list[i].Lock));
			if (!list[i].Lock)
			{
				BodyPartData bodyPartData = PandoraSingleton<DataFactory>.Instance.InitData<BodyPartData>((int)list[i].BodyPartId);
				if (bodyPartData.UnitSlotId != UnitSlotId.NONE)
				{
					bodyParts[list[i].BodyPartId].SetRelatedItem(Items[(int)bodyPartData.UnitSlotId]);
				}
			}
		}
	}

	public Dictionary<SpellCurseId, int> GetCurseModifiers()
	{
		Dictionary<SpellCurseId, int> dictionary = new Dictionary<SpellCurseId, int>();
		RefreshActiveItems();
		GetEnchantmentsCurseModifiers(dictionary, Enchantments);
		for (int i = 0; i < ActiveItems.Count; i++)
		{
			GetEnchantmentsCurseModifiers(dictionary, ActiveItems[i].Enchantments);
			if (ActiveItems[i].RuneMark != null)
			{
				GetEnchantmentsCurseModifiers(dictionary, ActiveItems[i].RuneMark.Enchantments);
			}
		}
		for (int j = 0; j < Injuries.Count; j++)
		{
			GetEnchantmentsCurseModifiers(dictionary, Injuries[j].Enchantments);
		}
		for (int k = 0; k < Mutations.Count; k++)
		{
			GetEnchantmentsCurseModifiers(dictionary, Mutations[k].Enchantments);
		}
		return dictionary;
	}

	private void GetEnchantmentsCurseModifiers(Dictionary<SpellCurseId, int> modifiers, List<Enchantment> enchantments)
	{
		for (int i = 0; i < enchantments.Count; i++)
		{
			for (int j = 0; j < enchantments[i].CurseModifiers.Count; j++)
			{
				EnchantmentCurseModifierData enchantmentCurseModifierData = enchantments[i].CurseModifiers[j];
				if (modifiers.ContainsKey(enchantmentCurseModifierData.SpellCurseId))
				{
					Dictionary<SpellCurseId, int> dictionary;
					Dictionary<SpellCurseId, int> dictionary2 = dictionary = modifiers;
					SpellCurseId spellCurseId;
					SpellCurseId key = spellCurseId = enchantmentCurseModifierData.SpellCurseId;
					int num = dictionary[spellCurseId];
					dictionary2[key] = num + enchantmentCurseModifierData.RatioModifier;
				}
				else
				{
					modifiers[enchantmentCurseModifierData.SpellCurseId] = enchantmentCurseModifierData.RatioModifier;
				}
			}
		}
	}

	public Dictionary<InjuryId, int> GetInjuryModifiers()
	{
		injuryRollModifiers.Clear();
		GetEnchantmentsInjuryModifiers(injuryRollModifiers, Enchantments);
		RefreshActiveItems();
		for (int i = 0; i < ActiveItems.Count; i++)
		{
			GetEnchantmentsInjuryModifiers(injuryRollModifiers, ActiveItems[i].Enchantments);
			if (ActiveItems[i].RuneMark != null)
			{
				GetEnchantmentsInjuryModifiers(injuryRollModifiers, ActiveItems[i].RuneMark.Enchantments);
			}
		}
		for (int j = 0; j < Injuries.Count; j++)
		{
			GetEnchantmentsInjuryModifiers(injuryRollModifiers, Injuries[j].Enchantments);
		}
		for (int k = 0; k < Mutations.Count; k++)
		{
			GetEnchantmentsInjuryModifiers(injuryRollModifiers, Mutations[k].Enchantments);
		}
		return injuryRollModifiers;
	}

	private void GetEnchantmentsInjuryModifiers(Dictionary<InjuryId, int> injuryRollModifiers, List<Enchantment> enchantments)
	{
		for (int i = 0; i < enchantments.Count; i++)
		{
			for (int j = 0; j < enchantments[i].InjuryModifiers.Count; j++)
			{
				EnchantmentInjuryModifierData enchantmentInjuryModifierData = enchantments[i].InjuryModifiers[j];
				if (injuryRollModifiers.ContainsKey(enchantmentInjuryModifierData.InjuryId))
				{
					Dictionary<InjuryId, int> dictionary;
					Dictionary<InjuryId, int> dictionary2 = dictionary = injuryRollModifiers;
					InjuryId injuryId;
					InjuryId key = injuryId = enchantmentInjuryModifierData.InjuryId;
					int num = dictionary[injuryId];
					dictionary2[key] = num + enchantmentInjuryModifierData.RatioModifier;
				}
				else
				{
					injuryRollModifiers[enchantmentInjuryModifierData.InjuryId] = enchantmentInjuryModifierData.RatioModifier;
				}
			}
		}
	}

	private void ApplyInjury(Injury injury)
	{
		for (int i = 0; i < injury.AttributeModifiers.Count; i++)
		{
			InjuryJoinAttributeData injuryJoinAttributeData = injury.AttributeModifiers[i];
			AddToAttribute(injuryJoinAttributeData.AttributeId, injuryJoinAttributeData.Modifier);
			AddAttributeModifier(AttributeMod.Type.INJURY, injuryJoinAttributeData.AttributeId, injury.LocName, injuryJoinAttributeData.Modifier);
		}
		for (int j = 0; j < injury.Enchantments.Count; j++)
		{
			ApplyEnchantment(injury.Enchantments[j]);
		}
	}

	public bool HasInjury(InjuryId injuryId)
	{
		return UnitSave.injuries.IndexOf(injuryId, InjuryIdComparer.Instance) != -1;
	}

	public InjuryId GetInjury(UnitSlotId slotId)
	{
		if (slotId == UnitSlotId.SET2_MAINHAND || slotId == UnitSlotId.SET2_OFFHAND)
		{
			slotId -= 2;
		}
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].Data.UnitSlotId == slotId)
			{
				return Injuries[i].Data.Id;
			}
		}
		return InjuryId.NONE;
	}

	public bool IsInjuryRepeatLimitExceeded(InjuryData injuryData, bool post)
	{
		int num = 0;
		for (int i = 0; i < UnitSave.injuries.Count; i++)
		{
			if (UnitSave.injuries[i] == injuryData.Id)
			{
				num++;
			}
		}
		if (injuryData.RepeatLimit == -1)
		{
			return false;
		}
		if (injuryData.RepeatLimit == 1)
		{
			if (post)
			{
				return false;
			}
			return num >= injuryData.RepeatLimit;
		}
		if (injuryData.RepeatLimit == 2)
		{
			return num >= injuryData.RepeatLimit;
		}
		return injuryData.RepeatLimit != -1 && num > injuryData.RepeatLimit;
	}

	public bool IsInjuryAttributeLimitExceeded(InjuryData injuryData, bool checkRetire)
	{
		List<InjuryJoinAttributeData> list = PandoraSingleton<DataFactory>.Instance.InitData<InjuryJoinAttributeData>("fk_injury_id", ((int)injuryData.Id).ToConstantString());
		for (int i = 0; i < list.Count; i++)
		{
			InjuryJoinAttributeData injuryJoinAttributeData = list[i];
			if (injuryJoinAttributeData.Limit != -1)
			{
				int attribute = GetAttribute(injuryJoinAttributeData.AttributeId);
				if (attribute <= injuryJoinAttributeData.Limit && (!checkRetire || injuryJoinAttributeData.Retire))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<InjuryData> GetPossibleInjuries(List<InjuryId> toExcludes, Unit unit, Dictionary<InjuryId, int> injuryModifiers)
	{
		List<InjuryData> list = PandoraSingleton<DataFactory>.Instance.InitData<InjuryData>("released", "1");
		List<InjuryData> list2 = new List<InjuryData>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			if (CanRollInjury(list[i], toExcludes, injuryModifiers))
			{
				list2.Add(list[i]);
			}
		}
		return list2;
	}

	public bool CanRollInjury(InjuryData injury, List<InjuryId> toExcludes, Dictionary<InjuryId, int> injuryModifiers)
	{
		int value = 0;
		injuryModifiers.TryGetValue(injury.Id, out value);
		return toExcludes.IndexOf(injury.Id, InjuryIdComparer.Instance) == -1 && CanStackInjury(injury) && injury.Ratio + value > 0;
	}

	private bool CanStackInjury(InjuryData injuryData)
	{
		if (injuryData.Id == InjuryId.AMNESIA && Rank == Constant.GetInt(ConstantId.MAX_UNIT_RANK))
		{
			return false;
		}
		if (injuryData.Id == InjuryId.DEAD || !HasInjury(injuryData.Id))
		{
			return true;
		}
		bool flag = true;
		return flag & (!IsInjuryRepeatLimitExceeded(injuryData, post: false) && !IsInjuryAttributeLimitExceeded(injuryData, checkRetire: false));
	}

	public void ClearInjuries()
	{
		Injuries.Clear();
		UnitSave.injuries.Clear();
		UnitSave.lastInjuryDate = 0;
		UnitSave.injuredTime = 0;
		UnitSave.injuryPaid = false;
	}

	public bool AddInjury(InjuryData injury, int day, List<Item> removedItems, bool isHireUnit = false, int overrideInjuryTime = -1)
	{
		UnitSave.injuries.Add(injury.Id);
		Injury item = new Injury(injury.Id, this);
		Injuries.Add(item);
		Logger.AddHistory(day, EventLogger.LogEvent.INJURY, (int)injury.Id);
		if (IsInjuryAttributeLimitExceeded(injury, checkRetire: true) || IsInjuryRepeatLimitExceeded(injury, post: true))
		{
			PandoraDebug.LogDebug("Injury Added... Unit Dead or Retired");
			return false;
		}
		for (int num = Mutations.Count - 1; num >= 0; num--)
		{
			if (Mutations[num].HasBodyPart(injury.BodyPartId))
			{
				for (int num2 = UnitSave.mutations.Count - 1; num2 >= 0; num2--)
				{
					if (UnitSave.mutations[num2] == Mutations[num].Data.Id)
					{
						UnitSave.mutations.RemoveAt(num2);
					}
				}
				Mutations.RemoveAt(num);
			}
		}
		UpdateAttributesAndCheckBackPack(removedItems);
		RefreshActiveItems();
		if (injury.UnitSlotId != UnitSlotId.NONE)
		{
			ReequipWeapons(removedItems);
		}
		if (overrideInjuryTime == -1)
		{
			overrideInjuryTime = injury.Duration;
		}
		if (!isHireUnit && overrideInjuryTime > 0)
		{
			UnitSave.lastInjuryDate = day;
			UnitSave.injuryPaid = false;
			if (overrideInjuryTime > UnitSave.injuredTime)
			{
				UnitSave.injuredTime = overrideInjuryTime;
			}
		}
		return true;
	}

	public int GetTreatmentCost()
	{
		int result = 0;
		if (!UnitSave.injuryPaid)
		{
			UnitCostData unitCost = GetUnitCost();
			result = UnitSave.injuredTime * unitCost.Treatment;
		}
		return result;
	}

	public void TreatmentPaid()
	{
		UnitSave.injuryPaid = true;
		Logger.RemoveLastHistory(EventLogger.LogEvent.NO_TREATMENT);
		Logger.AddHistory(UnitSave.lastInjuryDate + UnitSave.injuredTime, EventLogger.LogEvent.RECOVERY, (int)UnitSave.injuries[UnitSave.injuries.Count - 1]);
		if (GetUpkeepOwned() > 0)
		{
			int date = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + Constant.GetInt(ConstantId.UPKEEP_DAYS_WITHOUT_PAY);
			Logger.AddHistory(date, EventLogger.LogEvent.LEFT, 0);
		}
	}

	public void UpdateInjury()
	{
		if (UnitSave.injuredTime > 0 && UnitSave.injuryPaid)
		{
			UnitSave.injuredTime = Mathf.Max(UnitSave.injuredTime - 1, 0);
		}
	}

	public void UpdateSkillTraining()
	{
		if (UnitSave.skillInTrainingId != 0)
		{
			UnitSave.trainingTime--;
			if (UnitSave.trainingTime <= 0)
			{
				EndLearnSkill();
			}
		}
	}

	private void ApplyMutation(Mutation mutation)
	{
		for (int i = 0; i < mutation.AttributeModifiers.Count; i++)
		{
			MutationAttributeData mutationAttributeData = mutation.AttributeModifiers[i];
			if ((mutationAttributeData.UnitActionIdTrigger == UnitActionId.NONE && mutationAttributeData.SkillIdTrigger == SkillId.NONE) || (ActiveSkill != null && ActiveSkill.UnitActionId != 0 && ActiveSkill.UnitActionId == mutationAttributeData.UnitActionIdTrigger) || (ActiveSkill != null && ActiveSkill.Id != 0 && ActiveSkill.Id == mutationAttributeData.SkillIdTrigger))
			{
				AddToAttribute(mutationAttributeData.AttributeId, mutationAttributeData.Modifier);
				AddAttributeModifier(AttributeMod.Type.MUTATION, mutationAttributeData.AttributeId, mutation.LocName, mutationAttributeData.Modifier);
			}
		}
		for (int j = 0; j < mutation.Enchantments.Count; j++)
		{
			ApplyEnchantment(mutation.Enchantments[j]);
		}
	}

	public MutationId GetMutationId(UnitSlotId slotId)
	{
		if (slotId == UnitSlotId.SET2_MAINHAND || slotId == UnitSlotId.SET2_OFFHAND)
		{
			slotId -= 2;
		}
		for (int i = 0; i < Mutations.Count; i++)
		{
			if (Mutations[i].GroupData.UnitSlotId == slotId)
			{
				return Mutations[i].Data.Id;
			}
		}
		return MutationId.NONE;
	}

	public Mutation GetMutation(UnitSlotId slotId)
	{
		if (slotId == UnitSlotId.SET2_MAINHAND || slotId == UnitSlotId.SET2_OFFHAND)
		{
			slotId -= 2;
		}
		for (int i = 0; i < Mutations.Count; i++)
		{
			if (Mutations[i].GroupData.UnitSlotId == slotId)
			{
				return Mutations[i];
			}
		}
		return null;
	}

	public bool HasMutation(MutationId id)
	{
		for (int i = 0; i < Mutations.Count; i++)
		{
			if (Mutations[i].Data.Id == id)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearMutations()
	{
		Mutations.Clear();
		UnitSave.mutations.Clear();
	}

	public Mutation AddRandomMutation(List<Item> previousItems)
	{
		List<UnitJoinMutationData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinMutationData>("fk_unit_id", ((int)Id).ToConstantString());
		List<MutationData> list2 = new List<MutationData>();
		for (int i = 0; i < list.Count; i++)
		{
			MutationData mutationData = PandoraSingleton<DataFactory>.Instance.InitData<MutationData>((int)list[i].MutationId);
			bool flag = true;
			for (int j = 0; j < Mutations.Count; j++)
			{
				if (!flag)
				{
					break;
				}
				if (mutationData.MutationGroupId == Mutations[j].GroupData.Id)
				{
					flag = false;
				}
			}
			if (flag)
			{
				list2.Add(mutationData);
			}
		}
		MutationData randomRatio = MutationData.GetRandomRatio(list2, PandoraSingleton<GameManager>.Instance.LocalTyche);
		return AddMutation(randomRatio.Id, previousItems);
	}

	public Mutation AddMutation(MutationId mutationId, List<Item> removeItems)
	{
		UnitSave.mutations.Add(mutationId);
		Mutation mutation = new Mutation(mutationId, this);
		Mutations.Add(mutation);
		for (int num = Injuries.Count - 1; num >= 0; num--)
		{
			if (mutation.HasBodyPart(Injuries[num].Data.BodyPartId))
			{
				UnitSave.injuries.RemoveAt(num);
				Injuries.RemoveAt(num);
			}
		}
		PandoraDebug.LogInfo("Mutation " + mutation.Data.Id + " added to " + Name);
		if (mutation.GroupData.UnitSlotId != UnitSlotId.NONE)
		{
			ReequipWeapons(removeItems);
		}
		if (BothArmsMutated())
		{
			for (int i = 6; i < Items.Count; i++)
			{
				removeItems.AddRange(EquipItem((UnitSlotId)i, ItemId.NONE));
			}
		}
		UpdateAttributesAndCheckBackPack(removeItems);
		return mutation;
	}

	public int GetCRC()
	{
		int num = 0;
		num += attributes[113];
		num += attributes[6];
		num += attributes[123];
		num += attributes[11];
		num += attributes[12];
		num += attributes[144];
		num += attributes[141];
		num += attributes[140];
		num += attributes[146];
		num += attributes[142];
		num += attributes[143];
		num += attributes[145];
		num += attributes[139];
		for (int i = 0; i < Enchantments.Count; i++)
		{
			num = (int)(num + Enchantments[i].Id);
		}
		return num;
	}

	public bool HasNoRange()
	{
		if (Items[(int)ActiveWeaponSlot] != null && Items[(int)ActiveWeaponSlot].TypeData != null && !Items[(int)ActiveWeaponSlot].TypeData.IsRange)
		{
			return !Items[(int)InactiveWeaponSlot].TypeData.IsRange;
		}
		return false;
	}
}
