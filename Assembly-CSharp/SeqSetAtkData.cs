using UnityEngine;
using WellFired;

[USequencerEvent("Mordheim/SetAtkData")]
[USequencerFriendlyName("SetAtkData")]
public class SeqSetAtkData : USEventBase
{
	public AttackResultId attackResult;

	public UnitStateId defenderStatus;

	public UnitStateId defenderStartingStatus;

	public bool nextHitCritical;

	public string atkTypeLabel;

	public string atkOutcomeLabel;

	public string atkFlyingLabel;

	public SeqSetAtkData()
		: this()
	{
	}

	public override void FireEvent()
	{
		if (!(Application.loadedLevelName != "sequence"))
		{
			UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
			focusedUnit.SetCurrentAction(SkillId.BASE_ATTACK);
			UnitController defenderCtrlr = focusedUnit.defenderCtrlr;
			if (defenderStartingStatus == UnitStateId.NONE)
			{
				defenderCtrlr.animator.Play(AnimatorIds.idle);
				defenderCtrlr.animator.SetInteger(AnimatorIds.unit_state, 0);
				defenderCtrlr.unit.SetStatus(UnitStateId.NONE);
			}
			else
			{
				defenderCtrlr.animator.Play(AnimatorIds.kneeling_stunned);
				defenderCtrlr.animator.SetInteger(AnimatorIds.unit_state, 2);
				defenderCtrlr.unit.SetStatus(UnitStateId.STUNNED);
			}
			focusedUnit.attackResultId = attackResult;
			defenderCtrlr.unit.PreviousStatus = defenderCtrlr.unit.Status;
			defenderCtrlr.unit.SetStatus(defenderStatus);
			focusedUnit.criticalHit = nextHitCritical;
			focusedUnit.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById(atkTypeLabel), null);
			focusedUnit.currentActionData.SetActionOutcome(atkOutcomeLabel);
			defenderCtrlr.flyingLabel = PandoraSingleton<LocalizationManager>.Instance.GetStringById(atkFlyingLabel);
			defenderCtrlr.Hide(hide: false, force: true);
			focusedUnit.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
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
