using WellFired;

[USequencerEvent("Mordheim/SendActionLabel")]
[USequencerFriendlyName("SendActionLabel")]
public class SeqSendActionLabel : USEventBase
{
	public SeqSendActionLabel()
		: this()
	{
	}

	public override void FireEvent()
	{
		UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
		if (!string.IsNullOrEmpty(focusedUnit.currentActionData.name) && (focusedUnit.Imprint.State == MapImprintStateId.VISIBLE || (focusedUnit.defenderCtrlr != null && focusedUnit.defenderCtrlr.Imprint.State == MapImprintStateId.VISIBLE)))
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_ACTION, focusedUnit);
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
