using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WellFired;

[USequencerFriendlyName("InitSpellDebug")]
[USequencerEvent("Mordheim/InitSpellDebug")]
public class SeqInitSpellDebug : USEventBase
{
    public bool area;

    public bool success;

    public AttackResultId result;

    public UnitStateId defenderStatus;

    public UnitStateId defenderStartingStatus;

    public bool isCloseToTarget;

    public SeqInitSpellDebug()
        : this()
    {
    }

    public override void FireEvent()
    {
        if (!(Application.loadedLevelName != "sequence"))
        {
            UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
            focusedUnit.SetCurrentAction(focusedUnit.actionStatus.First((ActionStatus x) => x.skillData.SkillTypeId == SkillTypeId.SPELL_ACTION && ((area && x.fxData.SequenceId == SequenceId.SPELL_AREA) || (!area && x.fxData.SequenceId == SequenceId.SPELL_POINT))).SkillId);
            UnitController defenderCtrlr = focusedUnit.defenderCtrlr;
            if (defenderStartingStatus == UnitStateId.NONE)
            {
                focusedUnit.defenderCtrlr.animator.Play(AnimatorIds.idle);
                focusedUnit.defenderCtrlr.animator.SetInteger(AnimatorIds.unit_state, 0);
                focusedUnit.defenderCtrlr.unit.SetStatus(UnitStateId.NONE);
            }
            else
            {
                focusedUnit.defenderCtrlr.animator.Play(AnimatorIds.kneeling_stunned);
                focusedUnit.defenderCtrlr.animator.SetInteger(AnimatorIds.unit_state, 2);
                focusedUnit.defenderCtrlr.unit.SetStatus(UnitStateId.STUNNED);
            }
            defenderCtrlr.unit.SetStatus(defenderStatus);
            focusedUnit.currentSpellTargetPosition = ((focusedUnit.CurrentAction.skillData.Range != 0) ? defenderCtrlr.BonesTr[BoneId.RIG_PELVIS].position : focusedUnit.BonesTr[BoneId.RIG_PELVIS].position);
            focusedUnit.defenders = new List<UnitController>();
            focusedUnit.defenders.Add(defenderCtrlr);
            focusedUnit.defenderCtrlr.attackResultId = result;
            SpellCasting spellCasting = (SpellCasting)focusedUnit.StateMachine.GetState(28);
            spellCasting.spellSuccess = success;
        }
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
