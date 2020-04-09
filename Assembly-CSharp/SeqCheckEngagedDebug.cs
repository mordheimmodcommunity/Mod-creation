using UnityEngine;
using WellFired;

[USequencerEvent("Mordheim/CheckEngagedDebug")]
[USequencerFriendlyName("CheckEngagedDebug")]
public class SeqCheckEngagedDebug : USEventBase
{
	public SeqCheckEngagedDebug()
		: this()
	{
	}

	public override void FireEvent()
	{
	}

	public override void ProcessEvent(float runningTime)
	{
		if (!(Application.loadedLevelName != "sequence"))
		{
			UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
			if (Vector3.SqrMagnitude(focusedUnit.transform.position - focusedUnit.defenderCtrlr.transform.position) <= 4f)
			{
				focusedUnit.SetAnimSpeed(0f);
				PandoraSingleton<MissionManager>.Instance.CamManager.GetComponent<CameraAnim>().Stop();
			}
		}
	}

	public override void EndEvent()
	{
		((USEventBase)this).EndEvent();
	}
}
