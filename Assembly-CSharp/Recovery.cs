using UnityEngine;

public class Recovery : ICheapState
{
	private UnitController unitCtrlr;

	public Recovery(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: true);
		if (unitCtrlr.unit.Status == UnitStateId.NONE)
		{
			OnSeqDone();
			return;
		}
		unitCtrlr.unit.PreviousStatus = unitCtrlr.unit.Status;
		unitCtrlr.unit.SetStatus(UnitStateId.NONE);
		unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_GET_UP);
		unitCtrlr.SetCurrentAction(SkillId.BASE_GETUP);
		unitCtrlr.CurrentAction.Activate();
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction.LocalizedName, PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/stun_recovery", cached: true));
		PandoraSingleton<MissionManager>.Instance.PlaySequence("recovery", unitCtrlr, OnSeqDone);
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
		unitCtrlr.SetStatusFX();
		unitCtrlr.WaitForAction(UnitController.State.STUPIDITY);
		unitCtrlr.ReapplyOnEngage();
		unitCtrlr.CheckAllAlone();
	}
}
