using WellFired;

[USequencerFriendlyName("TriggerTrapUnitAction")]
[USequencerEvent("Mordheim/TriggerTrapUnitAction")]
public class SeqTriggerTrapUnitAction : USEventBase
{
    public SeqTriggerTrapUnitAction()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
        focusedUnit.activeTrigger.ActionOnUnit(focusedUnit);
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
