using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ActionStatus
{
    public enum AvailableReason
    {
        NONE,
        FRIENDLY_ZONE,
        NOT_ENOUGH_POINTS,
        NOT_ENGAGED,
        ENGAGED,
        MELEE_WEAPON_NEEDED,
        RANGE_WEAPON_NEEDED,
        WEAPON_NOT_RELOADED,
        WOUND_COST,
        WEAPON_ALREADY_RELOADED,
        INTERACTION_NOT_IN_RANGE,
        TARGET_NOT_IN_RANGE,
        NEED_WEAPON_TYPE,
        WEAPON_LOCKED,
        FLEE_NO_ROOM,
        DISENGAGE_NO_ROOM,
        DELAY,
        SWITCH_WEAPON,
        BLOCKED_BY_ENCHANTMENT,
        TRAP_TOO_CLOSE
    }

    public UnitActionData actionData;

    public SkillData skillData;

    public SkillFxData fxData;

    private List<SkillPerformSkillData> skillActionData;

    private UnitController unitCtrlr;

    public bool waitForConfirmation;

    private int extraOffensePts;

    private int extraStrategyPts;

    private StringBuilder logBldr = new StringBuilder();

    private List<SkillItemData> requiredItemsData;

    public UnitActionId ActionId => skillData.UnitActionId;

    public SkillId SkillId => skillData.Id;

    public Item LinkedItem
    {
        get;
        private set;
    }

    public ZoneAoeId ZoneAoeId => skillData.ZoneAoeId;

    public bool Available
    {
        get;
        private set;
    }

    public AvailableReason NotAvailableReason
    {
        get;
        private set;
    }

    public EnchantmentId BlockedByEnchantment
    {
        get;
        private set;
    }

    public List<UnitController> Targets
    {
        get;
        private set;
    }

    public List<Destructible> Destructibles
    {
        get;
        private set;
    }

    public string Name
    {
        get
        {
            string text = null;
            if (IsInteractive && unitCtrlr.interactivePoint != null)
            {
                text = unitCtrlr.interactivePoint.GetLocAction();
            }
            return (!string.IsNullOrEmpty(text)) ? text : skillData.Name;
        }
    }

    public int RangeMin
    {
        get
        {
            if (IsShootAction())
            {
                Item item = unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item;
                if (item.TargetingId != TargetingId.CONE && item.TargetingId != TargetingId.LINE)
                {
                    return item.RangeMin;
                }
            }
            return skillData.RangeMin;
        }
    }

    public int RangeMax
    {
        get
        {
            if (IsShootAction())
            {
                return unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.RangeMax;
            }
            return skillData.Range + ((skillData.Range != 0 && skillData.SkillTypeId == SkillTypeId.SPELL_ACTION) ? unitCtrlr.unit.RangeBonusSpell : 0);
        }
    }

    public TargetingId TargetingId => (!IsShootAction()) ? skillData.TargetingId : unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.TargetingId;

    public bool TargetAlly => (!IsShootAction()) ? skillData.TargetAlly : (unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.TargetAlly || skillData.TargetAlly);

    public int Radius => (!IsShootAction()) ? skillData.Radius : unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.Radius;

    public bool IsMastery => skillData.SkillQualityId == SkillQualityId.MASTER_QUALITY;

    public int StrategyPoints => Mathf.Max(unitCtrlr.unit.GetStratCostModifier(SkillId, ActionId, skillData.SpellTypeId) + skillData.StrategyPoints + extraStrategyPts, 0);

    public int OffensePoints => Mathf.Max(unitCtrlr.unit.GetOffCostModifier(SkillId, ActionId, skillData.SpellTypeId) + skillData.OffensePoints + extraOffensePts, 0);

    public string LocalizedDescription => SkillHelper.GetLocalizedDescription(skillData.Id);

    public bool IsInteractive
    {
        get
        {
            switch (ActionId)
            {
                case UnitActionId.LEAP:
                case UnitActionId.SEARCH:
                case UnitActionId.ACTIVATE:
                case UnitActionId.CLIMB:
                case UnitActionId.JUMP:
                    if (unitCtrlr.interactivePoint != null)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }

    public string LocalizedName
    {
        get
        {
            string text = null;
            if (IsInteractive && unitCtrlr.interactivePoint != null)
            {
                text = unitCtrlr.interactivePoint.GetLocAction();
            }
            return (!string.IsNullOrEmpty(text)) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById(text) : SkillHelper.GetLocalizedName(skillData);
        }
    }

    public string LocalizedNotAvailableReason
    {
        get
        {
            if (NotAvailableReason != 0)
            {
                if (NotAvailableReason == AvailableReason.BLOCKED_BY_ENCHANTMENT)
                {
                    if (PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentData>((int)BlockedByEnchantment).NoDisplay)
                    {
                        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("enchant_desc_block_tutorial_actions");
                    }
                    return PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_action_" + NotAvailableReason.ToLowerString(), "#enchant_title_" + BlockedByEnchantment.ToLowerString());
                }
                return PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_action_" + NotAvailableReason.ToLowerString());
            }
            return string.Empty;
        }
    }

    public ActionStatus(SkillData data, UnitController ctrlr)
    {
        Init(data, ctrlr);
    }

    public ActionStatus(Item item, UnitController ctrlr)
    {
        LinkedItem = item;
        SkillData data = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)item.ConsumableData.SkillId);
        Init(data, ctrlr);
    }

    private void Init(SkillData data, UnitController ctrlr)
    {
        skillData = data;
        unitCtrlr = ctrlr;
        Available = false;
        actionData = PandoraSingleton<DataFactory>.Instance.InitData<UnitActionData>((int)ActionId);
        requiredItemsData = PandoraSingleton<DataFactory>.Instance.InitData<SkillItemData>("fk_skill_id", ((int)SkillId).ToConstantString());
        List<SkillFxData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillFxData>("fk_skill_id", ((int)SkillId).ToConstantString());
        fxData = ((list == null || list.Count <= 0) ? null : list[0]);
        skillActionData = PandoraSingleton<DataFactory>.Instance.InitData<SkillPerformSkillData>("fk_skill_id", ((int)SkillId).ToConstantString());
        Targets = new List<UnitController>();
        Destructibles = new List<Destructible>();
    }

    public void SetOwner(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    private int CountAvailableEngagedUnits()
    {
        int num = 0;
        for (int i = 0; i < unitCtrlr.EngagedUnits.Count; i++)
        {
            if (unitCtrlr.EngagedUnits[i].unit.IsAvailable())
            {
                num++;
            }
        }
        return num;
    }

    public void UpdateAvailable()
    {
        Available = true;
        BlockedByEnchantment = EnchantmentId.NONE;
        Targets.Clear();
        Destructibles.Clear();
        switch (ActionId)
        {
            case UnitActionId.INTERACTION:
                return;
            case UnitActionId.DISENGAGE:
                if (CountAvailableEngagedUnits() == 0)
                {
                    extraOffensePts = 0;
                    extraStrategyPts = 0;
                    extraOffensePts = -OffensePoints;
                    extraStrategyPts = -StrategyPoints;
                }
                else
                {
                    extraOffensePts = 0;
                    extraStrategyPts = 0;
                }
                break;
        }
        EnchantmentId enchantmentId;
        if (unitCtrlr.AICtrlr != null && skillData.AiProof)
        {
            Available = false;
        }
        else if (PandoraSingleton<MissionManager>.Instance.interruptingUnit != null && PandoraSingleton<MissionManager>.Instance.interruptingUnit != unitCtrlr)
        {
            NotAvailableReason = AvailableReason.NONE;
            Available = false;
        }
        else if (unitCtrlr.IsInFriendlyZone)
        {
            NotAvailableReason = AvailableReason.FRIENDLY_ZONE;
            Available = false;
        }
        else if (!unitCtrlr.unit.HasEnoughPoints(StrategyPoints, OffensePoints))
        {
            NotAvailableReason = AvailableReason.NOT_ENOUGH_POINTS;
            Available = false;
        }
        else if (unitCtrlr.unit.IsSkillBlocked(skillData, out enchantmentId))
        {
            NotAvailableReason = AvailableReason.BLOCKED_BY_ENCHANTMENT;
            BlockedByEnchantment = enchantmentId;
            Available = false;
        }
        else if (skillData.NotEngaged && unitCtrlr.Engaged)
        {
            NotAvailableReason = AvailableReason.ENGAGED;
            Available = false;
        }
        else if (skillData.Engaged && ((ActionId != UnitActionId.MELEE_ATTACK && !unitCtrlr.Engaged) || (ActionId == UnitActionId.MELEE_ATTACK && !unitCtrlr.Engaged && unitCtrlr.triggeredDestructibles.Count == 0)))
        {
            NotAvailableReason = AvailableReason.NOT_ENGAGED;
            Available = false;
        }
        else if (skillData.NeedCloseSet && !unitCtrlr.HasClose())
        {
            NotAvailableReason = AvailableReason.MELEE_WEAPON_NEEDED;
            Available = false;
        }
        else if (skillData.NeedRangeSet && !unitCtrlr.HasRange())
        {
            NotAvailableReason = AvailableReason.RANGE_WEAPON_NEEDED;
            Available = false;
        }
        else if (skillData.WeaponLoaded && unitCtrlr.GetCurrentShots() <= 0)
        {
            NotAvailableReason = AvailableReason.WEAPON_NOT_RELOADED;
            Available = false;
        }
        else if (unitCtrlr.unit.CurrentWound < skillData.WoundsCostMax)
        {
            NotAvailableReason = AvailableReason.WOUND_COST;
            Available = false;
        }
        else if (skillData.TrapTypeId != 0)
        {
            List<MapImprint> mapImprints = PandoraSingleton<MissionManager>.Instance.MapImprints;
            int teamIdx = unitCtrlr.GetWarband().teamIdx;
            for (int i = 0; i < mapImprints.Count; i++)
            {
                if (mapImprints[i].Trap != null && mapImprints[i].Trap.TeamIdx == teamIdx && Vector3.SqrMagnitude(unitCtrlr.transform.position - mapImprints[i].Trap.transform.position) < 1f)
                {
                    NotAvailableReason = AvailableReason.TRAP_TOO_CLOSE;
                    Available = false;
                    break;
                }
            }
        }
        else if (SetTargets() && Targets.Count == 0 && Destructibles.Count == 0 && TargetingId == TargetingId.SINGLE_TARGET)
        {
            NotAvailableReason = AvailableReason.TARGET_NOT_IN_RANGE;
            Available = false;
        }
        else if (requiredItemsData.Count != 0)
        {
            bool flag = false;
            for (int j = 0; j < requiredItemsData.Count; j++)
            {
                if (flag)
                {
                    break;
                }
                SkillItemData skillItemData = requiredItemsData[j];
                flag = unitCtrlr.unit.HasItemActive(skillItemData.ItemId);
                if (flag && skillItemData.MutationId != 0)
                {
                    flag = unitCtrlr.unit.HasMutation(skillItemData.MutationId);
                }
            }
            if (!flag)
            {
                NotAvailableReason = AvailableReason.NEED_WEAPON_TYPE;
                Available = false;
            }
        }
        if (ActionId == UnitActionId.END_TURN && !Available && unitCtrlr.unit.CurrentStrategyPoints == 0)
        {
            Available = true;
        }
        if (!Available)
        {
            return;
        }
        switch (ActionId)
        {
            case UnitActionId.RELOAD:
                if (unitCtrlr.GetCurrentShots() >= unitCtrlr.GetMaxShots())
                {
                    NotAvailableReason = AvailableReason.WEAPON_ALREADY_RELOADED;
                    Available = false;
                }
                break;
            case UnitActionId.SWITCH_WEAPONSET:
                if (!unitCtrlr.CanSwitchWeapon())
                {
                    NotAvailableReason = AvailableReason.SWITCH_WEAPON;
                    Available = false;
                }
                break;
            case UnitActionId.LEAP:
            case UnitActionId.SEARCH:
            case UnitActionId.ACTIVATE:
            case UnitActionId.CLIMB:
            case UnitActionId.JUMP:
                {
                    bool flag2 = false;
                    for (int k = 0; k < unitCtrlr.interactivePoints.Count; k++)
                    {
                        if (unitCtrlr.interactivePoints[k].GetUnitActionIds(unitCtrlr).IndexOf(ActionId, UnitActionIdComparer.Instance) != -1)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    Available = (Available && flag2);
                    break;
                }
            case UnitActionId.CONSUMABLE:
                Available &= (unitCtrlr.unit.Data.UnitSizeId != UnitSizeId.LARGE && !unitCtrlr.unit.BothArmsMutated());
                break;
            case UnitActionId.DELAY:
                if (PandoraSingleton<MissionManager>.Instance.CurrentLadderIdx >= PandoraSingleton<MissionManager>.Instance.GetLadderLastValidPosition())
                {
                    NotAvailableReason = AvailableReason.DELAY;
                    Available = false;
                }
                break;
            case UnitActionId.FLEE:
                if (CountAvailableEngagedUnits() <= 0 || !unitCtrlr.CanDisengage())
                {
                    NotAvailableReason = AvailableReason.FLEE_NO_ROOM;
                    Available = false;
                }
                break;
            case UnitActionId.DISENGAGE:
                if (!unitCtrlr.CanDisengage())
                {
                    NotAvailableReason = AvailableReason.DISENGAGE_NO_ROOM;
                    Available = false;
                }
                break;
        }
    }

    public void Select()
    {
        if (actionData.Confirmation)
        {
            if (!waitForConfirmation)
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRMATION_BOX);
                unitCtrlr.SetAnimSpeed(0f);
                unitCtrlr.SetFixed(fix: true);
                waitForConfirmation = true;
                OnActionSelected();
            }
            else
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRM);
                Confirm();
            }
        }
        else
        {
            Confirm();
        }
    }

    public void Cancel()
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CANCEL, v1: true);
        OnActionConfirmedCancelled(confirmed: false);
        waitForConfirmation = false;
    }

    private void OnActionSelected()
    {
        UnitActionId actionId = ActionId;
        if (actionId == UnitActionId.STANCE && (SkillId == SkillId.BASE_STANCE_AMBUSH || SkillId == SkillId.PROWL || SkillId == SkillId.PROWL_MSTR || SkillId == SkillId.ONSLAUGHT || SkillId == SkillId.ONSLAUGHT_MSTR))
        {
            float @float = Constant.GetFloat((unitCtrlr.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? ConstantId.MELEE_RANGE_NORMAL : ConstantId.MELEE_RANGE_LARGE);
            unitCtrlr.chargeCircle.gameObject.SetActive(value: true);
            unitCtrlr.chargeCircle.Set((float)unitCtrlr.unit.AmbushMovement + @float, unitCtrlr.CapsuleRadius);
        }
    }

    private void OnActionConfirmedCancelled(bool confirmed)
    {
        UnitActionId actionId = ActionId;
        if (actionId == UnitActionId.STANCE && (SkillId == SkillId.BASE_STANCE_AMBUSH || SkillId == SkillId.PROWL || SkillId == SkillId.PROWL_MSTR || SkillId == SkillId.ONSLAUGHT || SkillId == SkillId.ONSLAUGHT_MSTR))
        {
            unitCtrlr.chargeCircle.gameObject.SetActive(value: false);
        }
    }

    public void RemovePoints()
    {
        unitCtrlr.unit.RemovePoints(StrategyPoints, OffensePoints);
    }

    private void Confirm()
    {
        OnActionConfirmedCancelled(confirmed: true);
        if (Available)
        {
            PandoraDebug.LogInfo("Confirming Skill " + SkillId, "ACTION");
            waitForConfirmation = false;
            unitCtrlr.SetCurrentAction(SkillId);
            SetTargets();
            switch (ActionId)
            {
                case UnitActionId.SHOOT:
                case UnitActionId.AIM:
                case UnitActionId.MELEE_ATTACK:
                case UnitActionId.CHARGE:
                case UnitActionId.SKILL:
                case UnitActionId.SPELL:
                case UnitActionId.CONSUMABLE:
                    GoToTargetingState();
                    break;
                case UnitActionId.LEAP:
                case UnitActionId.SEARCH:
                case UnitActionId.ACTIVATE:
                case UnitActionId.CLIMB:
                case UnitActionId.JUMP:
                case UnitActionId.INTERACTION:
                    unitCtrlr.StateMachine.ChangeState(23);
                    break;
                default:
                    unitCtrlr.SendSkill(SkillId);
                    break;
            }
        }
    }

    public void Activate()
    {
        PandoraDebug.LogInfo("Activating Skill " + SkillId, "ACTION");
        RemovePoints();
        if (skillData.WoundsCostMin != 0)
        {
            int damage = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(skillData.WoundsCostMin, skillData.WoundsCostMax);
            unitCtrlr.ComputeDirectWound(damage, byPassArmor: true, unitCtrlr);
        }
        unitCtrlr.LastActivatedAction = this;
        for (int i = 0; i < unitCtrlr.defenders.Count; i++)
        {
            unitCtrlr.defenders[i].ResetAttackResult();
            unitCtrlr.defenders[i].unit.ResetEnchantsChanged();
        }
        unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.NONE);
        unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_ACTION, SkillId, ActionId);
        PandoraDebug.LogDebug("Activate Action" + ActionId, "ACTION", unitCtrlr);
        if (unitCtrlr.defenders.Count == 0)
        {
            if (ActionId != UnitActionId.END_TURN)
            {
                PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(unitCtrlr, CombatLogger.LogMessage.PERFORM_SKILL, unitCtrlr.GetLogName(), LocalizedName);
            }
        }
        else
        {
            logBldr.Length = 0;
            for (int j = 0; j < unitCtrlr.defenders.Count; j++)
            {
                if (unitCtrlr.defenderCtrlr.IsImprintVisible())
                {
                    logBldr.AppendFormat(unitCtrlr.defenders[j].GetLogName());
                    if (j < unitCtrlr.defenders.Count - 1)
                    {
                        logBldr.Append(",");
                    }
                }
            }
            string text = logBldr.ToString();
            if (string.IsNullOrEmpty(text))
            {
                text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_nobody");
            }
            PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(unitCtrlr, CombatLogger.LogMessage.PERFORM_SKILL_TARGETS, unitCtrlr.GetLogName(), LocalizedName, text);
        }
        switch (ActionId)
        {
            case UnitActionId.SKILL:
            case UnitActionId.CONSUMABLE:
                unitCtrlr.StateMachine.ChangeState(30);
                break;
            case UnitActionId.SPELL:
                unitCtrlr.StateMachine.ChangeState(28);
                break;
            case UnitActionId.SHOOT:
            case UnitActionId.AIM:
            case UnitActionId.OVERWATCH:
                unitCtrlr.StateMachine.ChangeState(31);
                break;
            case UnitActionId.MELEE_ATTACK:
                unitCtrlr.StateMachine.ChangeState(32);
                break;
            case UnitActionId.CHARGE:
            case UnitActionId.AMBUSH:
                unitCtrlr.LaunchMelee(UnitController.State.CHARGE);
                break;
            case UnitActionId.END_TURN:
                unitCtrlr.StateMachine.ChangeState(39);
                break;
            case UnitActionId.PERCEPTION:
                unitCtrlr.StateMachine.ChangeState(13);
                break;
            case UnitActionId.DELAY:
                unitCtrlr.StateMachine.ChangeState(22);
                break;
            case UnitActionId.DISENGAGE:
                unitCtrlr.StateMachine.ChangeState(20);
                break;
            case UnitActionId.FLEE:
                unitCtrlr.StateMachine.ChangeState(40);
                break;
            case UnitActionId.RELOAD:
                unitCtrlr.StateMachine.ChangeState(19);
                break;
            case UnitActionId.STANCE:
                unitCtrlr.StateMachine.ChangeState(35);
                break;
            case UnitActionId.SWITCH_WEAPONSET:
                unitCtrlr.StateMachine.ChangeState(21);
                break;
            case UnitActionId.LEAP:
            case UnitActionId.CLIMB:
            case UnitActionId.JUMP:
                unitCtrlr.StateMachine.ChangeState(46);
                break;
            case UnitActionId.ACTIVATE:
                unitCtrlr.StateMachine.ChangeState(16);
                break;
            case UnitActionId.SEARCH:
                unitCtrlr.StateMachine.ChangeState(14);
                break;
            case UnitActionId.FLY:
                unitCtrlr.StateMachine.ChangeState(49);
                break;
        }
    }

    public bool SetTargets()
    {
        Targets.Clear();
        Destructibles.Clear();
        if (skillData == null)
        {
            PandoraDebug.LogWarning("Getting targets for Skill " + SkillId + " but there is no skillData available", "ACTION");
            return false;
        }
        bool flag = skillActionData != null && skillActionData.Count > 0;
        int idx = unitCtrlr.GetWarband().idx;
        if (ActionId == UnitActionId.CHARGE)
        {
            unitCtrlr.SetChargeTargets(unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() && unitCtrlr.IsPlayed());
            Targets.AddRange(unitCtrlr.chargeTargets);
        }
        else
        {
            List<UnitController> list = (!skillData.Engaged) ? PandoraSingleton<MissionManager>.Instance.GetAllUnits() : unitCtrlr.EngagedUnits;
            for (int i = 0; i < list.Count; i++)
            {
                UnitController unitController = list[i];
                if (((flag && unitController.unit.IsAvailable()) || (!flag && unitController.unit.Status != UnitStateId.OUT_OF_ACTION)) && (skillData.EnchantmentIdRequiredTarget == EnchantmentId.NONE || unitController.unit.HasEnchantment(skillData.EnchantmentIdRequiredTarget)))
                {
                    if (skillData.TargetSelf && unitController == unitCtrlr)
                    {
                        Targets.Add(unitController);
                    }
                    if (TargetAlly && unitController != unitCtrlr && unitController.GetWarband().idx == idx)
                    {
                        Targets.Add(unitController);
                    }
                    if (skillData.TargetEnemy && unitController != unitCtrlr && unitController.GetWarband().idx != idx)
                    {
                        Targets.Add(unitController);
                    }
                }
            }
            if (!skillData.Engaged)
            {
                if (TargetingId != TargetingId.AREA && TargetingId != TargetingId.AREA_GROUND)
                {
                    bool flag2 = IsShootAction();
                    float requiredPerc = (!flag2) ? Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC) : Constant.GetFloat(ConstantId.RANGE_SHOOT_REQUIRED_PERC);
                    for (int num = Targets.Count - 1; num >= 0; num--)
                    {
                        if (Targets[num] != unitCtrlr && !unitCtrlr.IsInRange(Targets[num], RangeMin, RangeMax + Radius, requiredPerc, flag2, flag2, skillData.BoneIdTarget))
                        {
                            Targets.RemoveAt(num);
                        }
                    }
                }
                else
                {
                    for (int num2 = Targets.Count - 1; num2 >= 0; num2--)
                    {
                        if (Targets[num2] != unitCtrlr && Vector3.Magnitude(unitCtrlr.transform.position - Targets[num2].transform.position) > (float)(RangeMax + Radius) + Targets[num2].CapsuleRadius)
                        {
                            Targets.RemoveAt(num2);
                        }
                    }
                }
            }
        }
        switch (ActionId)
        {
            case UnitActionId.MELEE_ATTACK:
                Destructibles.AddRange(unitCtrlr.triggeredDestructibles);
                break;
            case UnitActionId.SHOOT:
            case UnitActionId.AIM:
            case UnitActionId.SPELL:
                {
                    if ((ActionId != UnitActionId.SPELL || skillData.WoundMax <= 0 || skillData.EnchantmentIdRequiredTarget != 0) && ActionId == UnitActionId.SPELL)
                    {
                        break;
                    }
                    Vector3 src = unitCtrlr.transform.position + Vector3.up * 1.4f;
                    for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.triggerPoints.Count; j++)
                    {
                        if (PandoraSingleton<MissionManager>.Instance.triggerPoints[j] is Destructible)
                        {
                            Destructible destructible = (Destructible)PandoraSingleton<MissionManager>.Instance.triggerPoints[j];
                            if (destructible.IsInRange(src, RangeMax + Radius))
                            {
                                Destructibles.Add(destructible);
                            }
                        }
                    }
                    break;
                }
        }
        if (Targets.Count > 0 && flag)
        {
            SkillId skillIdPerformed = skillActionData[0].SkillIdPerformed;
            ActionStatus actionStatus = null;
            for (int num3 = Targets.Count - 1; num3 >= 0; num3--)
            {
                actionStatus = Targets[num3].GetAction(skillIdPerformed);
                if (actionStatus != null)
                {
                    actionStatus.UpdateAvailable();
                    if (actionStatus.Available)
                    {
                        continue;
                    }
                }
                Targets.RemoveAt(num3);
            }
        }
        return true;
    }

    public int GetRoll(bool updateModifiers = false)
    {
        bool flag = unitCtrlr.unit.GetUnitTypeId() == UnitTypeId.IMPRESSIVE;
        int num = -1;
        switch (ActionId)
        {
            case UnitActionId.SHOOT:
            case UnitActionId.AIM:
            case UnitActionId.OVERWATCH:
                num = unitCtrlr.GetRangeHitRoll(updateModifiers);
                if (num == 100 && unitCtrlr.destructibleTarget != null)
                {
                    return num;
                }
                break;
            case UnitActionId.MELEE_ATTACK:
            case UnitActionId.CHARGE:
            case UnitActionId.AMBUSH:
                num = unitCtrlr.GetMeleeHitRoll(updateModifiers);
                if (num == 100 && unitCtrlr.destructibleTarget != null)
                {
                    return num;
                }
                break;
            case UnitActionId.SPELL:
                num = unitCtrlr.GetSpellCastingRoll(skillData.SpellTypeId, updateModifiers);
                break;
            case UnitActionId.CLIMB:
                if (unitCtrlr.activeActionDest != null)
                {
                    if (flag)
                    {
                        return 100;
                    }
                    switch (unitCtrlr.GetFallHeight(unitCtrlr.activeActionDest.actionId))
                    {
                        case 3:
                            num = unitCtrlr.unit.ClimbRoll3;
                            if (updateModifiers)
                            {
                                unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.CLIMB_ROLL_3));
                            }
                            break;
                        case 6:
                            num = unitCtrlr.unit.ClimbRoll6;
                            if (updateModifiers)
                            {
                                unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.CLIMB_ROLL_6));
                            }
                            break;
                        case 9:
                            num = unitCtrlr.unit.ClimbRoll9;
                            if (updateModifiers)
                            {
                                unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.CLIMB_ROLL_9));
                            }
                            break;
                    }
                    break;
                }
                return -1;
            case UnitActionId.JUMP:
                if (unitCtrlr.activeActionDest != null)
                {
                    if (flag)
                    {
                        return 100;
                    }
                    switch (unitCtrlr.GetFallHeight(unitCtrlr.activeActionDest.actionId))
                    {
                        case 3:
                            num = unitCtrlr.unit.JumpDownRoll3;
                            if (updateModifiers)
                            {
                                unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.JUMP_DOWN_ROLL_3));
                            }
                            break;
                        case 6:
                            num = unitCtrlr.unit.JumpDownRoll6;
                            if (updateModifiers)
                            {
                                unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.JUMP_DOWN_ROLL_6));
                            }
                            break;
                        case 9:
                            num = unitCtrlr.unit.JumpDownRoll9;
                            if (updateModifiers)
                            {
                                unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.JUMP_DOWN_ROLL_9));
                            }
                            break;
                    }
                    break;
                }
                return -1;
            case UnitActionId.LEAP:
                if (flag)
                {
                    return 100;
                }
                num = unitCtrlr.unit.LeapRoll;
                if (updateModifiers)
                {
                    unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.LEAP_ROLL));
                }
                break;
            case UnitActionId.STANCE:
                switch (SkillId)
                {
                    case SkillId.BASE_STANCE_DODGE:
                        num = unitCtrlr.unit.DodgeRoll;
                        if (updateModifiers)
                        {
                            unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.DODGE_ROLL));
                        }
                        break;
                    case SkillId.BASE_STANCE_PARRY:
                        num = unitCtrlr.unit.ParryingRoll;
                        if (updateModifiers)
                        {
                            unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.PARRYING_ROLL));
                        }
                        break;
                }
                break;
            case UnitActionId.PERCEPTION:
                num = unitCtrlr.unit.PerceptionRoll;
                if (updateModifiers)
                {
                    unitCtrlr.CurrentRollModifiers.AddRange(unitCtrlr.unit.attributeModifiers.GetOrNull(AttributeId.PERCEPTION_ROLL));
                }
                break;
        }
        if (num != -1)
        {
            num = Mathf.Clamp(num, 0, Constant.GetInt(ConstantId.MAX_ROLL));
        }
        return num;
    }

    public UnitController GetTarget()
    {
        switch (ActionId)
        {
            case UnitActionId.CHARGE:
                if (unitCtrlr.defenderCtrlr != null)
                {
                    return unitCtrlr.defenderCtrlr;
                }
                if (unitCtrlr.chargeTargets.Count > 0)
                {
                    return unitCtrlr.chargeTargets[0];
                }
                return null;
            case UnitActionId.MELEE_ATTACK:
                if (unitCtrlr.defenderCtrlr != null)
                {
                    return unitCtrlr.defenderCtrlr;
                }
                if (unitCtrlr.EngagedUnits.Count > 0)
                {
                    return unitCtrlr.EngagedUnits[0];
                }
                return null;
            case UnitActionId.SHOOT:
            case UnitActionId.AIM:
                if (unitCtrlr.defenderCtrlr != null)
                {
                    return unitCtrlr.defenderCtrlr;
                }
                if (Targets.Count > 0)
                {
                    return Targets[0];
                }
                return null;
            case UnitActionId.AMBUSH:
            case UnitActionId.OVERWATCH:
                return unitCtrlr.defenderCtrlr;
            case UnitActionId.SPELL:
                return unitCtrlr.defenderCtrlr;
            default:
                return null;
        }
    }

    public bool HasDamage()
    {
        switch (ActionId)
        {
            case UnitActionId.SHOOT:
            case UnitActionId.AIM:
            case UnitActionId.MELEE_ATTACK:
            case UnitActionId.CHARGE:
            case UnitActionId.AMBUSH:
            case UnitActionId.OVERWATCH:
                return true;
            case UnitActionId.SPELL:
                return skillData.WoundMax > 0;
            case UnitActionId.LEAP:
            case UnitActionId.CLIMB:
            case UnitActionId.JUMP:
                return true;
            default:
                return skillData.WoundMax > 0;
        }
    }

    public int GetMinDamage(bool updateModifiers = false)
    {
        UnitController target = GetTarget();
        Unit target2 = (!(target != null)) ? null : target.unit;
        int result = skillData.WoundMin;
        switch (ActionId)
        {
            case UnitActionId.SHOOT:
            case UnitActionId.AIM:
            case UnitActionId.MELEE_ATTACK:
            case UnitActionId.CHARGE:
            case UnitActionId.AMBUSH:
            case UnitActionId.OVERWATCH:
                result = unitCtrlr.unit.GetWeaponDamageMin(target2, critical: false, skillData.BypassArmor, unitCtrlr.IsCharging);
                if (updateModifiers)
                {
                    unitCtrlr.CurrentDamageModifiers.AddRange(unitCtrlr.unit.GetWeaponDamageModifier(target2, skillData.BypassArmor, unitCtrlr.IsCharging));
                }
                break;
            case UnitActionId.SPELL:
                if (skillData.WoundMin > 0)
                {
                    result = unitCtrlr.unit.ApplySpellDamageModifier(SkillId, skillData.WoundMin, target2, skillData.SpellTypeId, skillData.BypassArmor);
                    if (updateModifiers)
                    {
                        unitCtrlr.CurrentDamageModifiers.Add(new AttributeMod(AttributeMod.Type.BASE, PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_base_spell_damage"), skillData.WoundMin, skillData.WoundMax));
                        unitCtrlr.CurrentDamageModifiers.AddRange(unitCtrlr.unit.GetSpellDamageModifier(SkillId, target2, skillData.SpellTypeId, skillData.BypassArmor));
                    }
                }
                break;
            case UnitActionId.LEAP:
            case UnitActionId.CLIMB:
            case UnitActionId.JUMP:
                if (unitCtrlr.activeActionDest != null)
                {
                    return unitCtrlr.GetFallHeight(unitCtrlr.activeActionDest.actionId);
                }
                break;
        }
        return result;
    }

    public int GetMaxDamage(bool critical = false)
    {
        UnitController target = GetTarget();
        Unit target2 = (!(target != null)) ? null : target.unit;
        int result = skillData.WoundMax;
        switch (ActionId)
        {
            case UnitActionId.SHOOT:
            case UnitActionId.AIM:
            case UnitActionId.MELEE_ATTACK:
            case UnitActionId.CHARGE:
            case UnitActionId.AMBUSH:
            case UnitActionId.OVERWATCH:
                return unitCtrlr.unit.GetWeaponDamageMax(target2, critical, skillData.BypassArmor, unitCtrlr.IsCharging);
            case UnitActionId.SPELL:
                if (skillData.WoundMax > 0)
                {
                    result = unitCtrlr.unit.ApplySpellDamageModifier(SkillId, skillData.WoundMax, target2, skillData.SpellTypeId, skillData.BypassArmor);
                }
                break;
            case UnitActionId.LEAP:
            case UnitActionId.CLIMB:
            case UnitActionId.JUMP:
                if (unitCtrlr.activeActionDest != null)
                {
                    result = unitCtrlr.GetFallHeight(unitCtrlr.activeActionDest.actionId) + 10;
                }
                break;
        }
        return result;
    }

    public Sprite GetIcon()
    {
        Sprite sprite = null;
        if (LinkedItem != null)
        {
            sprite = LinkedItem.GetIcon();
        }
        if (sprite == null)
        {
            sprite = GetIcon(skillData.Name);
        }
        if (sprite == null)
        {
            sprite = GetIcon(ActionId.ToLowerString());
        }
        return sprite;
    }

    public static Sprite GetIcon(string name)
    {
        Sprite sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/" + name.ToLowerString(), cached: true);
        if (sprite == null)
        {
            sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/" + name.ToLowerInvariant().Replace("base_", string.Empty).Replace("_mstr", string.Empty), cached: true);
        }
        if (sprite == null)
        {
        }
        return sprite;
    }

    private void GoToTargetingState()
    {
        switch (TargetingId)
        {
            case TargetingId.CONE:
                unitCtrlr.StateMachine.ChangeState(26);
                break;
            case TargetingId.AREA:
            case TargetingId.AREA_GROUND:
                unitCtrlr.StateMachine.ChangeState(25);
                break;
            case TargetingId.ARC:
                unitCtrlr.StateMachine.ChangeState(50);
                break;
            case TargetingId.SINGLE_TARGET:
                unitCtrlr.StateMachine.ChangeState(24);
                break;
            case TargetingId.LINE:
                unitCtrlr.StateMachine.ChangeState(27);
                break;
        }
    }

    public bool IsShootAction()
    {
        return skillData.UnitActionId == UnitActionId.SHOOT || skillData.UnitActionId == UnitActionId.AIM || skillData.UnitActionId == UnitActionId.OVERWATCH;
    }
}
