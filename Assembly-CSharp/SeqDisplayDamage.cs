using WellFired;

[USequencerFriendlyName("DisplayDamage")]
[USequencerEvent("Mordheim/DisplayDamage")]
public class SeqDisplayDamage : USEventBase
{
    public SequenceTargetId targetId;

    public SeqDisplayDamage()
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
            ((FlyingLabel)fl).Play(unitCtrlr.BonesTr[BoneId.RIG_HEAD].position, true, "com_dash_value", unitCtrlr.lastActionWounds.ToString());
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
