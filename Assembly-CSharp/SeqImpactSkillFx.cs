using Prometheus;
using WellFired;

[USequencerFriendlyName("ImpactSkillFx")]
[USequencerEvent("Mordheim/ImpactSkillFx")]
public class SeqImpactSkillFx : USEventBase
{
    public SeqImpactSkillFx()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
        if (focusedUnit.CurrentAction.fxData != null && focusedUnit.defenderCtrlr != null)
        {
            PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(focusedUnit.CurrentAction.fxData.ImpactFx, focusedUnit.defenderCtrlr, null, null);
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
