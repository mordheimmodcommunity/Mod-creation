using UnityEngine;
using WellFired;

[USequencerFriendlyName("InitSkillDebug")]
[USequencerEvent("Mordheim/InitSkillDebug")]
public class SeqInitSkillDebug : USEventBase
{
	public SeqInitSkillDebug()
		: this()
	{
	}

	public override void FireEvent()
	{
		if (Application.loadedLevelName != "sequence")
		{
			return;
		}
		UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
		SkillId currentAction = SkillId.NONE;
		for (int i = 0; i < focusedUnit.actionStatus.Count; i++)
		{
			if (focusedUnit.actionStatus[i].skillData != null && focusedUnit.actionStatus[i].skillData.SkillTypeId == SkillTypeId.SKILL_ACTION)
			{
				currentAction = focusedUnit.actionStatus[i].SkillId;
			}
		}
		focusedUnit.SetCurrentAction(currentAction);
	}

	public override void ProcessEvent(float runningTime)
	{
	}

	public override void EndEvent()
	{
		((USEventBase)this).EndEvent();
	}
}
