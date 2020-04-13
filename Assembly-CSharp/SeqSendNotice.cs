using System;
using WellFired;

[USequencerEvent("Mordheim/SendNotice")]
[USequencerFriendlyName("SendNotice")]
public class SeqSendNotice : USEventBase
{
    public string noticeName;

    public string action;

    public SequenceTargetId targetId;

    public SeqSendNotice()
        : this()
    {
    }

    public override void FireEvent()
    {
        Notices notices = (Notices)(int)Enum.Parse(typeof(Notices), noticeName);
        UnitController unitController = null;
        switch (targetId)
        {
            case SequenceTargetId.FOCUSED_UNIT:
                PandoraDebug.LogDebug("SequenceTargetId... Attacker / focusedUnit", "SEQUENCE");
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                break;
            case SequenceTargetId.DEFENDER:
                PandoraDebug.LogDebug("SequenceTargetId... Defender", "SEQUENCE");
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr;
                break;
        }
        if (unitController.Imprint.State != 0)
        {
            return;
        }
        switch (notices)
        {
            case Notices.PHASE_RECOVERY_CHECK:
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_ACTION, unitController);
                return;
            case Notices.REACTION_ACTION_OUTCOME:
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_ACTION_OUTCOME, unitController);
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, unitController);
                return;
        }
        if (string.IsNullOrEmpty(action))
        {
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
