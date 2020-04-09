using UnityEngine;

public class AIControlled : ICheapState
{
	private const int FAILED_MOVEMENT_MAX = 3;

	private const float MAX_PROCESS_TIME = 30f;

	private UnitController unitCtrlr;

	private float processTimer;

	public AIControlled(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.AICtrlr.RestartBT();
		unitCtrlr.AICtrlr.failedMove = 0;
		unitCtrlr.AICtrlr.UpdateVisibility();
		unitCtrlr.UpdateActionStatus(notice: false);
		PandoraSingleton<MissionManager>.Instance.TurnOffActionZones();
		processTimer = 0f;
		if (PandoraSingleton<GameManager>.Instance.IsFastForwarded)
		{
			Time.timeScale = 1.5f;
		}
	}

	void ICheapState.Exit(int iTo)
	{
		PandoraSingleton<GameManager>.Instance.ResetTimeScale();
		if (!unitCtrlr.GetComponent<Rigidbody>().isKinematic)
		{
			unitCtrlr.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		unitCtrlr.SetAnimSpeed(0f);
	}

	void ICheapState.Update()
	{
		processTimer += Time.deltaTime;
		if (unitCtrlr.AICtrlr.failedMove == 3 || processTimer > 30f)
		{
			PandoraDebug.LogInfo("AI turn finished because it failed to much movements or was thinking too much", "AI", unitCtrlr);
			unitCtrlr.SendSkill(SkillId.BASE_END_TURN);
		}
	}

	void ICheapState.FixedUpdate()
	{
		if (!unitCtrlr.IsAnimating())
		{
			unitCtrlr.AICtrlr.FixedUpdate();
		}
	}
}
