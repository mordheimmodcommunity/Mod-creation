using Prometheus;
using WellFired;

[USequencerEvent("Mordheim/LaunchSkillFx")]
[USequencerFriendlyName("LaunchSkillFx")]
public class SeqLaunchSkillFx : USEventBase
{
	public SeqLaunchSkillFx()
		: this()
	{
	}

	public override void FireEvent()
	{
		UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
		if (focusedUnit.CurrentAction.fxData != null)
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(focusedUnit.CurrentAction.fxData.LaunchFx, focusedUnit, null, null);
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
