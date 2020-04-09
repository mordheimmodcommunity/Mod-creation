using UnityEngine;

public class Charge : ICheapState
{
	private const float BLOCK_TIME = 1f;

	private const float MAX_CHARGE_TIME = 15f;

	private UnitController unitCtrlr;

	private Vector3 lastPosition;

	private float blockedTimer;

	private float chargeTime;

	public Charge(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraDebug.LogInfo("Charge Enter ", "UNIT_FLOW", unitCtrlr);
		unitCtrlr.SetFixed(fix: false);
		lastPosition = unitCtrlr.transform.position;
		blockedTimer = 0f;
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
		unitCtrlr.FaceTarget(unitCtrlr.defenderCtrlr.transform, force: true);
		PandoraSingleton<MissionManager>.Instance.PlaySequence("charge", unitCtrlr);
		chargeTime = 0f;
	}

	void ICheapState.Exit(int iTo)
	{
		PandoraSingleton<SequenceManager>.Instance.EndSequence();
		if (unitCtrlr.chargeFx != null)
		{
			unitCtrlr.chargeFx.DestroyFx(force: true);
			unitCtrlr.chargeFx = null;
		}
	}

	void ICheapState.Update()
	{
		chargeTime += Time.deltaTime;
	}

	void ICheapState.FixedUpdate()
	{
		if (chargeTime > 15f)
		{
			PandoraDebug.LogWarning("Charge failed due to too long charge", "CHARGE", unitCtrlr);
			PandoraSingleton<SequenceManager>.Instance.EndSequence();
			unitCtrlr.SendStartMove(unitCtrlr.transform.position, unitCtrlr.transform.rotation);
			return;
		}
		unitCtrlr.UpdateTargetsData();
		if (unitCtrlr.Engaged)
		{
			return;
		}
		unitCtrlr.FaceTarget(unitCtrlr.defenderCtrlr.transform, force: true);
		if (!PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() || !unitCtrlr.unit.IsAvailable())
		{
			return;
		}
		unitCtrlr.CheckEngaged(applyEnchants: true);
		if (unitCtrlr.Engaged)
		{
			PandoraSingleton<SequenceManager>.Instance.EndSequence();
			unitCtrlr.SetFixed(fix: true);
			unitCtrlr.SendEngaged(unitCtrlr.transform.position, unitCtrlr.transform.rotation, charge: true);
			return;
		}
		if (Vector3.SqrMagnitude(unitCtrlr.transform.position - lastPosition) < 0.005625f)
		{
			blockedTimer += Time.deltaTime;
			if (blockedTimer >= 1f)
			{
				PandoraDebug.LogWarning("Charge failed due to movement fail", "CHARGE", unitCtrlr);
				PandoraSingleton<SequenceManager>.Instance.EndSequence();
				unitCtrlr.SendStartMove(unitCtrlr.transform.position, unitCtrlr.transform.rotation);
				return;
			}
		}
		else
		{
			blockedTimer = 0f;
		}
		lastPosition = unitCtrlr.transform.position;
	}
}
