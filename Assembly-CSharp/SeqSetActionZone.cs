using UnityEngine;
using WellFired;

[USequencerFriendlyName("SetActionZone")]
[USequencerEvent("Mordheim/SetActionZone")]
public class SeqSetActionZone : USEventBase
{
	public UnitActionId actionId;

	public string currentAction = "action";

	public SeqSetActionZone()
		: this()
	{
	}

	public override void FireEvent()
	{
		if (!(Application.loadedLevelName != "sequence"))
		{
			UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
			focusedUnit.SetFixed(fix: false);
			focusedUnit.GetComponent<Rigidbody>().isKinematic = true;
			focusedUnit.animator.Play(AnimatorIds.idle);
			ActionZoneSeqHelper actionZoneHelper = PandoraSingleton<SequenceHelper>.Instance.GetActionZoneHelper(actionId);
			focusedUnit.interactivePoint = actionZoneHelper.actionZone;
			focusedUnit.transform.position = actionZoneHelper.actionZone.transform.position;
			focusedUnit.transform.rotation = actionZoneHelper.actionZone.transform.rotation;
			if (actionZoneHelper.actionDest != null)
			{
				ActionDestination actionDestination = new ActionDestination();
				actionDestination.actionId = actionId;
				actionDestination.destination = actionZoneHelper.actionDest;
				focusedUnit.activeActionDest = actionDestination;
			}
			if (actionZoneHelper.actionDef != null)
			{
				focusedUnit.defenderCtrlr.transform.position = actionZoneHelper.actionDef.transform.position;
				focusedUnit.defenderCtrlr.transform.rotation = actionZoneHelper.actionDef.transform.rotation;
			}
			focusedUnit.currentActionLabel = currentAction;
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
