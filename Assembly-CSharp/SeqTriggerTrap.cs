using WellFired;

[USequencerEvent("Mordheim/TriggerTrap")]
[USequencerFriendlyName("TriggerTrap")]
public class SeqTriggerTrap : USEventBase
{
	public SeqTriggerTrap()
		: this()
	{
	}

	public override void FireEvent()
	{
		UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
		if (focusedUnit.activeTrigger != null)
		{
			focusedUnit.activeTrigger.Trigger(focusedUnit);
		}
		else if (focusedUnit.currentZoneAoe != null)
		{
			focusedUnit.EnterZoneAoeAnim();
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
