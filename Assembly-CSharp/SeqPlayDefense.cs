using WellFired;

[USequencerFriendlyName("PlayDefense")]
[USequencerEvent("Mordheim/PlayDefense")]
public class SeqPlayDefense : USEventBase
{
    public SeqPlayDefense()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
        focusedUnit.PlayDefState(focusedUnit.attackResultId, (focusedUnit.unit.Status == UnitStateId.OUT_OF_ACTION) ? 1 : 0, focusedUnit.unit.Status);
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
