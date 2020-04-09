using UnityEngine;
using WellFired;

[USequencerFriendlyName("StartRun")]
[USequencerEvent("Mordheim/StartRun")]
public class SeqStartRun : USEventBase
{
	public SequenceTargetId targetId;

	public SeqStartRun()
		: this()
	{
	}

	public override void FireEvent()
	{
		UnitController unitController = null;
		switch (targetId)
		{
		case SequenceTargetId.FOCUSED_UNIT:
			unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
			break;
		case SequenceTargetId.DEFENDER:
			unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr;
			break;
		}
		unitController.SetAnimSpeed(1f);
		unitController.SetFixed(fix: false);
		unitController.GetComponent<Rigidbody>().isKinematic = false;
	}

	public override void ProcessEvent(float runningTime)
	{
	}

	public override void EndEvent()
	{
		((USEventBase)this).EndEvent();
	}
}
