using WellFired;

[USequencerFriendlyName("LaunchAction")]
[USequencerEvent("Mordheim/LaunchAction")]
public class SeqLaunchAction : USEventBase
{
    public SequenceTargetId targetId;

    public UnitActionId actionId;

    public bool success;

    public int variation;

    public UnitStateId stateId;

    public SeqLaunchAction()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController unitController = null;
        switch (targetId)
        {
            case SequenceTargetId.FOCUSED_UNIT:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                break;
            case SequenceTargetId.DEFENDER:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr;
                break;
        }
        unitController.LaunchAction(actionId, success, stateId, variation);
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
