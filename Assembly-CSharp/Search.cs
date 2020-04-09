using UnityEngine;

public class Search : ICheapState
{
	private UnitController unitCtrlr;

	public Search(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: true);
		if ((Object)(object)unitCtrlr.interactivePoint.Highlight != null)
		{
			unitCtrlr.FaceTarget(((Component)(object)unitCtrlr.interactivePoint.Highlight).transform, force: true);
		}
		else
		{
			unitCtrlr.FaceTarget(unitCtrlr.interactivePoint.transform, force: true);
		}
		PandoraSingleton<MissionManager>.Instance.ForceFocusedUnit(unitCtrlr);
		if (unitCtrlr.interactivePoint is SearchPoint)
		{
			((SearchPoint)unitCtrlr.interactivePoint).wasSearched |= unitCtrlr.IsPlayed();
			unitCtrlr.GetWarband().SearchOpened((SearchPoint)unitCtrlr.interactivePoint);
			((SearchPoint)unitCtrlr.interactivePoint).InitInteraction();
		}
		PandoraSingleton<MissionManager>.Instance.TurnTimer.Pause();
		unitCtrlr.searchVariation = (int)unitCtrlr.interactivePoint.anim;
		unitCtrlr.LaunchAction(UnitActionId.SEARCH, success: true, UnitStateId.NONE, unitCtrlr.searchVariation);
		if (PandoraSingleton<GameManager>.Instance.IsFastForwarded && unitCtrlr.unit.isAI)
		{
			Time.timeScale = 1.5f;
		}
	}

	void ICheapState.Exit(int iTo)
	{
		PandoraSingleton<GameManager>.Instance.ResetTimeScale();
	}

	void ICheapState.Update()
	{
		unitCtrlr.RegisterItems();
		if (unitCtrlr.AICtrlr == null)
		{
			unitCtrlr.StateMachine.ChangeState(15);
			return;
		}
		Inventory inventory = (Inventory)unitCtrlr.StateMachine.GetState(15);
		inventory.UpdateInventory();
		unitCtrlr.AICtrlr.GotoSearchMode();
		unitCtrlr.StateMachine.ChangeState(42);
	}

	void ICheapState.FixedUpdate()
	{
	}
}
