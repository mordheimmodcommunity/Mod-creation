using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseCombatAttack : ICheapState
{
    private UnitController unitCtrlr;

    private List<UnitController> targets = new List<UnitController>();

    private int autoDamages;

    public CloseCombatAttack(UnitController ctrler)
    {
        unitCtrlr = ctrler;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        autoDamages = 0;
        unitCtrlr.SetFixed(fix: true);
        PandoraSingleton<MissionManager>.Instance.HideCombatCircles();
        LaunchAttack();
    }

    void ICheapState.Exit(int iTo)
    {
        if (unitCtrlr.defenderCtrlr != null && unitCtrlr.defenderCtrlr != PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UPDATE_TARGET, unitCtrlr.defenderCtrlr, unitCtrlr.defenderCtrlr.unit.warbandIdx);
        }
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void LaunchAttack()
    {
        unitCtrlr.attackResultId = AttackResultId.MISS;
        unitCtrlr.criticalHit = false;
        unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
        unitCtrlr.flyingLabel = string.Empty;
        unitCtrlr.lastActionWounds = 0;
        bool flag = true;
        bool flag2 = false;
        bool flag3 = false;
        unitCtrlr.seqData.actionSuccess = true;
        unitCtrlr.seqData.action = (int)unitCtrlr.CurrentAction.ActionId;
        if (unitCtrlr.seqData.action == 20 || unitCtrlr.seqData.action == 58)
        {
            unitCtrlr.seqData.action = 19;
        }
        unitCtrlr.seqData.attackVariation = 0;
        unitCtrlr.seqData.emoteVariation = 0;
        if (unitCtrlr.CurrentAction.skillData.SkillTypeId != SkillTypeId.BASE_ACTION && unitCtrlr.CurrentAction.skillData.Id != SkillId.BASE_COUNTER_ATTACK && unitCtrlr.CurrentAction.skillData.Id != SkillId.BASE_FLEE_ATTACK && unitCtrlr.CurrentAction.skillData.Id != SkillId.BASE_ATTACK_FREE && unitCtrlr.CurrentAction.skillData.Id != SkillId.BASE_AMBUSH_ATTACK && unitCtrlr.CurrentAction.skillData.Id != SkillId.BASE_CHARGE_FREE)
        {
            unitCtrlr.seqData.emoteVariation = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(1, 5);
        }
        targets.Clear();
        targets.AddRange(unitCtrlr.defenders);
        for (int i = 0; i < targets.Count; i++)
        {
            unitCtrlr.defenderCtrlr = targets[i];
            unitCtrlr.defenderCtrlr.flyingLabel = string.Empty;
            unitCtrlr.defenderCtrlr.currentActionData.Reset();
            unitCtrlr.defenderCtrlr.attackResultId = AttackResultId.NONE;
            PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(unitCtrlr.defenderCtrlr.transform);
            unitCtrlr.defenderCtrlr.InitDefense(unitCtrlr);
            if (unitCtrlr.defenderCtrlr.unit.IsAvailable())
            {
                int roll = unitCtrlr.CurrentAction.GetRoll();
                flag = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, roll, AttributeId.COMBAT_MELEE_HIT_ROLL, reverse: false, apply: true, unitCtrlr.hitMod);
                if (!flag)
                {
                    if (roll > 75)
                    {
                        unitCtrlr.hitMod += Constant.GetInt(ConstantId.HIT_ROLL_MOD);
                    }
                    else if (roll > 50)
                    {
                        unitCtrlr.hitMod += Constant.GetInt(ConstantId.HIT_ROLL_MOD) / 2;
                    }
                    unitCtrlr.defenderCtrlr.attackResultId = AttackResultId.MISS;
                    unitCtrlr.defenderCtrlr.flyingLabel = PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_miss");
                    PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr.defenderCtrlr, string.Empty, v3: false, PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_miss"));
                }
            }
            else
            {
                PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(unitCtrlr, CombatLogger.LogMessage.STUNNED_HIT, unitCtrlr.defenderCtrlr.GetLogName());
            }
            if (flag)
            {
                unitCtrlr.hitMod = 0;
                unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_MELEE_HIT_SUCCESS);
                if (unitCtrlr.CurrentAction.ActionId == UnitActionId.AMBUSH || unitCtrlr.CurrentAction.ActionId == UnitActionId.CHARGE)
                {
                    unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_CHARGE_SUCCESS);
                }
            }
            if (flag && unitCtrlr.unit.DodgeBypass <= 0 && unitCtrlr.defenderCtrlr.unit.IsAvailable() && unitCtrlr.defenderCtrlr.unit.DodgeLeft > 0)
            {
                int value = unitCtrlr.defenderCtrlr.unit.DodgeRoll + unitCtrlr.unit.DodgeDefenderModifier;
                value = Mathf.Clamp(value, 1, Constant.GetInt(ConstantId.MAX_ROLL));
                flag2 = unitCtrlr.defenderCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, value, AttributeId.DODGE_ROLL);
                unitCtrlr.defenderCtrlr.unit.ConsumeEnchantments(EnchantmentConsumeId.DODGE);
                if (flag2)
                {
                    unitCtrlr.defenderCtrlr.unit.UpdateValidNextActionEnchantments();
                    unitCtrlr.defenderCtrlr.attackResultId = AttackResultId.DODGE;
                    unitCtrlr.defenderCtrlr.currentActionData.SetActionOutcome("act_effect_dodged");
                }
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr.defenderCtrlr, (!flag2) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_fail_param", "#retro_outcome_dodge") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_dodge"), flag2, (!flag2) ? string.Empty : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_dodge"));
            }
            if (flag && unitCtrlr.unit.ParryBypass <= 0 && !flag2 && unitCtrlr.defenderCtrlr.unit.IsAvailable() && unitCtrlr.defenderCtrlr.unit.ParryLeft > 0)
            {
                int value2 = unitCtrlr.defenderCtrlr.unit.ParryingRoll + unitCtrlr.unit.ParryDefenderModifier;
                value2 = Mathf.Clamp(value2, 1, Constant.GetInt(ConstantId.MAX_ROLL));
                flag3 = unitCtrlr.defenderCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, value2, AttributeId.PARRYING_ROLL);
                unitCtrlr.defenderCtrlr.unit.ConsumeEnchantments(EnchantmentConsumeId.PARRY);
                if (flag3)
                {
                    unitCtrlr.seqData.actionSuccess = false;
                    unitCtrlr.defenderCtrlr.unit.UpdateValidNextActionEnchantments();
                    unitCtrlr.defenderCtrlr.attackResultId = AttackResultId.PARRY;
                    unitCtrlr.defenderCtrlr.currentActionData.SetActionOutcome("act_effect_dodged");
                }
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr.defenderCtrlr, (!flag3) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_fail_param", "#retro_outcome_parry") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_parry"), flag3, (!flag3) ? string.Empty : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_parry"));
            }
            if (flag && !flag2 && !flag3)
            {
                unitCtrlr.ComputeWound();
            }
            unitCtrlr.seqData.attackVariation = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, 2);
        }
        for (int j = 0; j < unitCtrlr.destructTargets.Count; j++)
        {
            PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(unitCtrlr.destructTargets[j].transform);
            unitCtrlr.ComputeDestructibleWound(unitCtrlr.destructTargets[j]);
        }
        if (unitCtrlr.CurrentAction.fxData != null && unitCtrlr.CurrentAction.fxData.OverrideVariation)
        {
            unitCtrlr.seqData.attackVariation = unitCtrlr.CurrentAction.fxData.Variation;
        }
        else if (unitCtrlr.unit.Id == UnitId.MANTICORE)
        {
            Vector3 zero = Vector3.zero;
            for (int k = 0; k < targets.Count; k++)
            {
                zero += targets[k].transform.position;
            }
            zero /= (float)targets.Count;
            zero -= unitCtrlr.transform.position;
            float num = Vector3.Dot(unitCtrlr.transform.forward, zero);
            int num2 = (num < 0f) ? 1 : 0;
            num = Vector3.Dot(unitCtrlr.transform.right, zero);
            num2 += ((num < 0f) ? 2 : 0);
            unitCtrlr.seqData.attackVariation = num2;
        }
        autoDamages = unitCtrlr.unit.GetEnchantmentDamage(PandoraSingleton<MissionManager>.Instance.NetworkTyche, EnchantmentDmgTriggerId.ON_MELEE);
        autoDamages += unitCtrlr.unit.GetEnchantmentDamage(PandoraSingleton<MissionManager>.Instance.NetworkTyche, EnchantmentDmgTriggerId.ON_ATTACK);
        if (autoDamages > 0)
        {
            unitCtrlr.ComputeDirectWound(autoDamages, byPassArmor: true, unitCtrlr);
        }
        unitCtrlr.defenders.Clear();
        unitCtrlr.defenders.AddRange(targets);
        string sequence = "attack_right" + ((!flag3) ? string.Empty : "_fail");
        PandoraSingleton<MissionManager>.Instance.PlaySequence(sequence, unitCtrlr, OnSeqDone);
        unitCtrlr.StartCoroutine(WaitToShowOutcome());
    }

    private IEnumerator WaitToShowOutcome()
    {
        yield return new WaitForSeconds(0.5f);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, unitCtrlr);
    }

    private void OnSeqDone()
    {
        if (autoDamages > 0)
        {
            unitCtrlr.attackResultId = AttackResultId.HIT;
            unitCtrlr.Hit();
            unitCtrlr.StartCoroutine(WaitForHitAnim());
        }
        else
        {
            EndAttack();
        }
    }

    private IEnumerator WaitForHitAnim()
    {
        yield return new WaitForSeconds(1.5f);
        EndAttack();
    }

    private void EndAttack()
    {
        unitCtrlr.attackUsed++;
        for (int i = 0; i < unitCtrlr.defenders.Count; i++)
        {
            unitCtrlr.defenders[i].EndDefense();
        }
        if (unitCtrlr.unit.Status == UnitStateId.OUT_OF_ACTION)
        {
            unitCtrlr.KillUnit();
            if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
            {
                unitCtrlr.StateMachine.ChangeState(39);
                return;
            }
        }
        if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() && unitCtrlr.attackUsed < unitCtrlr.unit.AttackPerAction && unitCtrlr.defenderCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            LaunchAttack();
            return;
        }
        if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() && unitCtrlr.defenderCtrlr != null && unitCtrlr.defenderCtrlr.unit.IsAvailable() && !unitCtrlr.defenderCtrlr.Resurected && unitCtrlr.defenders.Count == 1)
        {
            unitCtrlr.defenderCtrlr.UpdateActionStatus(notice: false);
            if (unitCtrlr.defenderCtrlr.CanCounterAttack())
            {
                unitCtrlr.defenderCtrlr.StateMachine.ChangeState(34);
                unitCtrlr.WaitForAction(UnitController.State.START_MOVE);
                if (unitCtrlr.IsPlayed())
                {
                    PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_START_SINGLE_TARGETING);
                }
                return;
            }
        }
        if (unitCtrlr.defenderCtrlr != null && unitCtrlr.defenderCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION && unitCtrlr.defenderCtrlr.Fleeing)
        {
            unitCtrlr.ActionDone();
        }
        else
        {
            unitCtrlr.StateMachine.ChangeState(10);
        }
    }
}
