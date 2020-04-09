using UnityEngine;
using WellFired;

[USequencerFriendlyName("InitRangeDebug")]
[USequencerEvent("Mordheim/InitRangeDebug")]
public class SeqInitRangeDebug : USEventBase
{
	public AttackResultId atkResult;

	public UnitStateId defenderStatus;

	public UnitStateId defenderStartingStatus;

	public string atkOutcomeLabel;

	public string atkFlyingLabel;

	public string atkTypeLabel;

	public BoneId targetBone;

	public bool noCollision;

	public Transform missTarget;

	public bool isCloseToTarget;

	public SeqInitRangeDebug()
		: this()
	{
	}

	public override void FireEvent()
	{
		if (!(Application.loadedLevelName != "sequence"))
		{
			UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
			focusedUnit.SetCurrentAction(SkillId.BASE_SHOOT);
			focusedUnit.Equipments[4].Reload();
			if (focusedUnit.Equipments[5] != null)
			{
				focusedUnit.Equipments[5].Reload();
			}
			focusedUnit.SwitchWeapons(UnitSlotId.SET2_MAINHAND);
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
			focusedUnit.attackResultId = atkResult;
			focusedUnit.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById(atkTypeLabel), null);
			focusedUnit.currentActionData.SetActionOutcome(atkOutcomeLabel);
			focusedUnit.defenderCtrlr.unit.SetStatus(defenderStatus);
			focusedUnit.defenderCtrlr.flyingLabel = PandoraSingleton<LocalizationManager>.Instance.GetStringById(atkFlyingLabel);
			RangeCombatFire rangeCombatFire = (RangeCombatFire)focusedUnit.StateMachine.GetState(31);
			rangeCombatFire.bodyParts.Clear();
			rangeCombatFire.noHitCollisions.Clear();
			rangeCombatFire.targetsPos.Clear();
			rangeCombatFire.bodyParts.Add(focusedUnit.defenderCtrlr.BonesTr[targetBone]);
			rangeCombatFire.noHitCollisions.Add(noCollision);
			if (!noCollision)
			{
				rangeCombatFire.targetsPos.Add(rangeCombatFire.bodyParts[0].transform.position);
			}
			else if (missTarget != null)
			{
				rangeCombatFire.targetsPos.Add(missTarget.transform.position);
			}
			else
			{
				rangeCombatFire.targetsPos.Add(focusedUnit.defenderCtrlr.BonesTr[BoneId.RIG_HEAD].position + rangeCombatFire.missOffset);
			}
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
