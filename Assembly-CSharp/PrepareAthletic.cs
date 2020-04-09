using System.Collections.Generic;
using UnityEngine;

public class PrepareAthletic : ICheapState
{
	private UnitController unitCtrlr;

	public bool success;

	public AthleticAction actionId;

	public int height;

	private Vector3 targetPos;

	public PrepareAthletic(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		bool flag = unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE;
		unitCtrlr.SetFixed(fix: false);
		unitCtrlr.GetComponent<Rigidbody>().isKinematic = true;
		Quaternion rotation = default(Quaternion);
		rotation.SetLookRotation(unitCtrlr.activeActionDest.destination.transform.position - unitCtrlr.interactivePoint.transform.position, Vector3.up);
		rotation.x = 0f;
		rotation.z = 0f;
		unitCtrlr.transform.rotation = rotation;
		success = true;
		height = unitCtrlr.GetFallHeight(unitCtrlr.activeActionDest.actionId);
		AttributeId attributeId = AttributeId.NONE;
		switch (unitCtrlr.activeActionDest.actionId)
		{
		case UnitActionId.CLIMB_3M:
		case UnitActionId.CLIMB_6M:
		case UnitActionId.CLIMB_9M:
			actionId = AthleticAction.CLIMB;
			unitCtrlr.transform.position = unitCtrlr.interactivePoint.transform.position + unitCtrlr.interactivePoint.transform.forward * (0f - ((!flag) ? 0.05f : 1.5f));
			attributeId = AttributeId.CLIMB_ROLL_3;
			break;
		case UnitActionId.LEAP:
			actionId = AthleticAction.LEAP;
			unitCtrlr.transform.position = unitCtrlr.interactivePoint.transform.position + unitCtrlr.interactivePoint.transform.forward * (0f - ((!flag) ? 0.5f : 1.5f));
			attributeId = AttributeId.LEAP_ROLL;
			break;
		case UnitActionId.JUMP_3M:
		case UnitActionId.JUMP_6M:
		case UnitActionId.JUMP_9M:
			actionId = AthleticAction.JUMP;
			unitCtrlr.transform.position = unitCtrlr.interactivePoint.transform.position + unitCtrlr.interactivePoint.transform.forward * (0f - ((!flag) ? 0f : 0.5f));
			attributeId = AttributeId.JUMP_DOWN_ROLL_3;
			break;
		}
		int roll = unitCtrlr.CurrentAction.GetRoll();
		success = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, roll, attributeId);
		if (!success)
		{
			if (actionId == AthleticAction.CLIMB)
			{
				unitCtrlr.activeActionDest = unitCtrlr.activeActionDest.destination.GetJump();
				unitCtrlr.unit.AddEnchantment(EnchantmentId.CLIMB_FAIL_EFFECT, unitCtrlr.unit, original: false);
			}
			else if (actionId == AthleticAction.LEAP)
			{
				unitCtrlr.activeActionDest = ((ActionZone)unitCtrlr.interactivePoint).GetJump();
			}
		}
		else if (actionId == AthleticAction.LEAP)
		{
			height = 3;
		}
		else if (actionId == AthleticAction.CLIMB)
		{
			unitCtrlr.unit.RemoveEnchantments(EnchantmentId.CLIMB_FAIL_EFFECT);
		}
		List<UnitController> list = new List<UnitController>(unitCtrlr.activeActionDest.destination.PointsChecker.enemiesOnZone);
		if (success && list.Count > 0)
		{
			unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_ATHLETIC_SUCCESS_ENGAGED, unitCtrlr.CurrentAction.SkillId, unitCtrlr.activeActionDest.actionId);
		}
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			PandoraSingleton<MissionManager>.Instance.MoveUnitsOnActionZone(unitCtrlr, unitCtrlr.activeActionDest.destination.PointsChecker, unitCtrlr.activeActionDest.destination.PointsChecker.alliesOnZone, isEnemy: false);
			PandoraSingleton<MissionManager>.Instance.MoveUnitsOnActionZone(unitCtrlr, unitCtrlr.activeActionDest.destination.PointsChecker, unitCtrlr.activeActionDest.destination.PointsChecker.enemiesOnZone, isEnemy: true);
		}
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			unitCtrlr.SendAthletic();
		}
	}

	void ICheapState.FixedUpdate()
	{
	}
}
