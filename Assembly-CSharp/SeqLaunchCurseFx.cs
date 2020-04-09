using WellFired;

[USequencerEvent("Mordheim/LaunchCurseFx")]
[USequencerFriendlyName("LaunchCurseFx")]
public class SeqLaunchCurseFx : USEventBase
{
	public SeqLaunchCurseFx()
		: this()
	{
	}

	public override void FireEvent()
	{
		UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
		SpellCurse spellCurse = (SpellCurse)focusedUnit.StateMachine.GetState(29);
		spellCurse.LaunchFx();
	}

	public override void ProcessEvent(float runningTime)
	{
	}

	public override void EndEvent()
	{
		((USEventBase)this).EndEvent();
	}
}
