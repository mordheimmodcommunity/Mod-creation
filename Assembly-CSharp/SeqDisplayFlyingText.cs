using WellFired;

[USequencerEvent("Mordheim/DisplayFlyingText")]
[USequencerFriendlyName("DisplayFlyingText")]
public class SeqDisplayFlyingText : USEventBase
{
    public SequenceTargetId targetId;

    public SeqDisplayFlyingText()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController unitCtrlr = null;
        switch (targetId)
        {
            case SequenceTargetId.FOCUSED_UNIT:
                unitCtrlr = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                break;
            case SequenceTargetId.DEFENDER:
                unitCtrlr = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr;
                break;
        }
        PandoraSingleton<FlyingTextManager>.Instance.GetFlyingText(FlyingTextId.ACTION, delegate (FlyingText fl)
        {
            ((FlyingLabel)fl).Play(unitCtrlr.BonesTr[BoneId.RIG_HEAD].position, false, unitCtrlr.flyingLabel);
        });
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
