using Prometheus;
using WellFired;

[USequencerFriendlyName("LaunchFx")]
[USequencerEvent("Mordheim/LaunchFx")]
public class SeqLaunchFx : USEventBase
{
    public string fxName;

    public SeqLaunchFx()
        : this()
    {
    }

    public override void FireEvent()
    {
        PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(fxName, PandoraSingleton<MissionManager>.Instance.focusedUnit, null, null);
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
