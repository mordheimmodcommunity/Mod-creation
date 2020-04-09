using WellFired;

[USequencerEvent("Mordheim/LaunchEffectFx")]
[USequencerFriendlyName("LaunchEffectFx")]
public class SeqLaunchEffectFx : USEventBase
{
	public SeqLaunchEffectFx()
		: this()
	{
	}

	public override void FireEvent()
	{
		UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
		UpdateEffects updateEffects = (UpdateEffects)focusedUnit.StateMachine.GetState(3);
		updateEffects.TriggerEffect();
	}

	public override void ProcessEvent(float runningTime)
	{
	}

	public override void EndEvent()
	{
		((USEventBase)this).EndEvent();
	}
}
