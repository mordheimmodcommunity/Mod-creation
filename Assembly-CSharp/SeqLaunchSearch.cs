using WellFired;

[USequencerFriendlyName("LaunchSearch")]
[USequencerEvent("Mordheim/LaunchSearch")]
public class SeqLaunchSearch : USEventBase
{
    public SeqLaunchSearch()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
        if (focusedUnit != null)
        {
            focusedUnit.LaunchAction(UnitActionId.SEARCH, success: true, UnitStateId.NONE, focusedUnit.searchVariation);
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
