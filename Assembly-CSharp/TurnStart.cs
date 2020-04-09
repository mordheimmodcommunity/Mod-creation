using System.Collections.Generic;

public class TurnStart : ICheapState
{
	private UnitController unitCtrlr;

	private bool circlesSet;

	public TurnStart(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.ResetAtkUsed();
		unitCtrlr.defenderCtrlr = null;
		unitCtrlr.Fleeing = false;
		unitCtrlr.TurnStarted = true;
		if (unitCtrlr.AICtrlr != null)
		{
			unitCtrlr.AICtrlr.TurnStartCleanUp();
		}
		unitCtrlr.defenderCtrlr = null;
		unitCtrlr.LastActivatedAction = null;
		circlesSet = false;
		PandoraSingleton<MissionManager>.Instance.SetCombatCircles(unitCtrlr, delegate
		{
			circlesSet = true;
		});
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
		if (PandoraSingleton<MissionManager>.Instance.IsNavmeshUpdating || !circlesSet)
		{
			return;
		}
		unitCtrlr.UpdateTargetsData();
		if (!unitCtrlr.HasEnemyInSight())
		{
			unitCtrlr.unit.DestroyEnchantments(EnchantmentTriggerId.ON_NO_ENEMY_IN_SIGHT);
		}
		PandoraSingleton<MissionManager>.Instance.UpdateZoneAoeDurations(unitCtrlr);
		List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
		for (int i = 0; i < allUnits.Count; i++)
		{
			allUnits[i].unit.UpdateEnchantmentsDuration(unitCtrlr.unit);
		}
		unitCtrlr.unit.UpdateAttributes();
		unitCtrlr.unit.ResetPoints();
		PandoraSingleton<MissionManager>.Instance.TurnTimer.Reset();
		if (PandoraSingleton<Hermes>.Instance.IsHost())
		{
			PandoraSingleton<MissionManager>.Instance.interruptingUnit = null;
		}
		if (unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
		{
			if (unitCtrlr.IsMine())
			{
				PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, unitCtrlr.transform, transition: false, force: false, clearFocus: true, unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
				if (PandoraSingleton<Hermes>.Instance.IsConnected())
				{
					unitCtrlr.StartSync();
				}
			}
			if (unitCtrlr.IsPlayed())
			{
				unitCtrlr.Imprint.SetCurrent(current: true);
			}
		}
		unitCtrlr.nextState = ((unitCtrlr.unit.Status == UnitStateId.OUT_OF_ACTION) ? UnitController.State.TURN_FINISHED : UnitController.State.TURN_MESSAGE);
	}

	void ICheapState.FixedUpdate()
	{
	}
}
