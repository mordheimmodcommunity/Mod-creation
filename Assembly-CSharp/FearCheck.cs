using UnityEngine;

public class FearCheck : ICheapState
{
	private UnitController unitCtrlr;

	private bool success;

	public FearCheck(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_fear_roll"), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/fear", cached: true));
		int fearRoll = unitCtrlr.unit.FearRoll;
		success = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, fearRoll, AttributeId.FEAR_ROLL);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr, string.Empty, success, (!success) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_fail") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_success"));
		PandoraSingleton<MissionManager>.Instance.PlaySequence("moral_check", unitCtrlr, OnSeqDone);
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void OnSeqDone()
	{
		unitCtrlr.ActionDone();
	}
}
