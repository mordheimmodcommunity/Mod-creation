using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeCombatFire : ICheapState
{
	private const string SEQ_NAME = "{0}_close_attack";

	private const float CLOSE_DISTANCE = 20f;

	private UnitController unitCtrlr;

	public Vector3 missOffset = new Vector3(0.4f, 0.4f, 0f);

	public List<MonoBehaviour> targets = new List<MonoBehaviour>();

	public List<Vector3> targetsPos = new List<Vector3>();

	public List<Transform> bodyParts = new List<Transform>();

	public List<bool> noHitCollisions = new List<bool>();

	private int autoDamages;

	public RangeCombatFire(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		if (unitCtrlr.CurrentAction.fxData != null && unitCtrlr.CurrentAction.fxData.HitFx != string.Empty)
		{
			unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].projectile.fxHitObstacle = unitCtrlr.CurrentAction.fxData.HitFx;
		}
		unitCtrlr.UpdateTargetsData();
		unitCtrlr.SetFixed(fix: true);
		PandoraSingleton<MissionManager>.Instance.HideCombatCircles();
		targets.Clear();
		targetsPos.Clear();
		bodyParts.Clear();
		noHitCollisions.Clear();
		if (unitCtrlr.defenderCtrlr != null)
		{
			unitCtrlr.FaceTarget(unitCtrlr.defenderCtrlr.transform);
			for (int i = 0; i < unitCtrlr.defenders.Count; i++)
			{
				targets.Add(unitCtrlr.defenders[i]);
			}
		}
		for (int j = 0; j < targets.Count; j++)
		{
			unitCtrlr.defenderCtrlr = (UnitController)targets[j];
			SetTargetAttackResult();
		}
		for (int k = 0; k < unitCtrlr.destructTargets.Count; k++)
		{
			targets.Add(unitCtrlr.destructTargets[k]);
			SetDestructibleResult(unitCtrlr.destructTargets[k]);
		}
		autoDamages = unitCtrlr.unit.GetEnchantmentDamage(PandoraSingleton<MissionManager>.Instance.NetworkTyche, EnchantmentDmgTriggerId.ON_RANGE);
		autoDamages += unitCtrlr.unit.GetEnchantmentDamage(PandoraSingleton<MissionManager>.Instance.NetworkTyche, EnchantmentDmgTriggerId.ON_ATTACK);
		if (autoDamages > 0)
		{
			unitCtrlr.ComputeDirectWound(autoDamages, byPassArmor: true, unitCtrlr);
		}
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRM);
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
		string sequence = string.Format("{0}_close_attack", (unitCtrlr.CurrentAction.SkillId != SkillId.BASE_SHOOT) ? "aiming" : "range");
		PandoraSingleton<MissionManager>.Instance.PlaySequence(sequence, unitCtrlr, OnSeqDone);
	}

	void ICheapState.Exit(int iTo)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] is UnitController)
			{
				((UnitController)targets[i]).EndDefense();
			}
		}
		if (unitCtrlr.defenderCtrlr != null && unitCtrlr.defenderCtrlr != PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UPDATE_TARGET, unitCtrlr.defenderCtrlr, unitCtrlr.defenderCtrlr.unit.warbandIdx);
		}
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void SetTargetAttackResult()
	{
		unitCtrlr.defenderCtrlr.currentActionData.Reset();
		unitCtrlr.defenderCtrlr.InitDefense(unitCtrlr, face: false);
		unitCtrlr.defenderCtrlr.beenShot = true;
		PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(unitCtrlr.defenderCtrlr.transform);
		int roll = unitCtrlr.CurrentAction.GetRoll();
		if (!unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, roll, AttributeId.COMBAT_RANGE_HIT_ROLL, reverse: false, apply: true, unitCtrlr.hitMod))
		{
			if (roll > 75)
			{
				unitCtrlr.hitMod += Constant.GetInt(ConstantId.HIT_ROLL_MOD);
			}
			else if (roll > 50)
			{
				unitCtrlr.hitMod += Constant.GetInt(ConstantId.HIT_ROLL_MOD) / 2;
			}
			unitCtrlr.attackResultId = AttackResultId.MISS;
			unitCtrlr.defenderCtrlr.attackResultId = AttackResultId.MISS;
			unitCtrlr.defenderCtrlr.flyingLabel = PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_miss");
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr.defenderCtrlr, string.Empty, v3: false, PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_miss"));
		}
		else
		{
			unitCtrlr.hitMod = 0;
			unitCtrlr.ComputeWound();
		}
		Transform transform = null;
		Vector3 zero = Vector3.zero;
		bool item = false;
		TargetData targetData = unitCtrlr.GetTargetData(unitCtrlr.defenderCtrlr);
		if (unitCtrlr.attackResultId < AttackResultId.HIT_NO_WOUND)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < targetData.boneTargetRangeBlockingUnit.Count; i++)
			{
				if (!targetData.boneTargetRangeBlockingUnit[i].hitBone)
				{
					list.Add(targetData.boneTargetRangeBlockingUnit[i].hitPoint);
				}
			}
			if (list.Count == 0)
			{
				zero = PandoraSingleton<MissionManager>.Instance.CamManager.OrientOffset(unitCtrlr.defenderCtrlr.transform, missOffset);
				zero += unitCtrlr.defenderCtrlr.BonesTr[BoneId.RIG_HEAD].position;
				item = true;
			}
			else
			{
				zero = list[PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list.Count)];
			}
		}
		else
		{
			BoneId boneIdTarget = unitCtrlr.CurrentAction.skillData.BoneIdTarget;
			if (boneIdTarget != 0)
			{
				for (int j = 0; j < unitCtrlr.defenderCtrlr.boneTargets.Count; j++)
				{
					if (unitCtrlr.defenderCtrlr.boneTargets[j].bone == boneIdTarget)
					{
						transform = unitCtrlr.defenderCtrlr.boneTargets[j].transform;
					}
				}
			}
			else
			{
				List<BoneTarget> list2 = new List<BoneTarget>();
				for (int k = 0; k < targetData.boneTargetRangeBlockingUnit.Count; k++)
				{
					if (targetData.boneTargetRangeBlockingUnit[k].hitBone)
					{
						list2.Add(unitCtrlr.defenderCtrlr.boneTargets[k]);
					}
				}
				transform = list2[PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list2.Count)].transform;
			}
			zero = transform.position;
		}
		targetsPos.Add(zero);
		bodyParts.Add(transform);
		noHitCollisions.Add(item);
		if (unitCtrlr.IsPlayed() && !unitCtrlr.defenderCtrlr.IsPlayed())
		{
			Vector3 position = unitCtrlr.transform.position;
			float y = position.y;
			Vector3 position2 = unitCtrlr.defenderCtrlr.transform.position;
			if (y - position2.y >= 8.5f)
			{
				PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.RANGE_9M);
			}
		}
	}

	private void SetDestructibleResult(Destructible dest)
	{
		PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(dest.transform);
		unitCtrlr.ComputeDestructibleWound(dest);
		targetsPos.Add(dest.transform.position + Vector3.up * 1f);
		bodyParts.Add(null);
		noHitCollisions.Add(item: false);
	}

	private void OnSeqDone()
	{
		if (autoDamages > 0)
		{
			unitCtrlr.attackResultId = AttackResultId.HIT;
			unitCtrlr.Hit();
			if (unitCtrlr.unit.Status == UnitStateId.OUT_OF_ACTION)
			{
				unitCtrlr.KillUnit();
			}
			unitCtrlr.StartCoroutine(Wait());
		}
		else
		{
			EndAction();
		}
	}

	private IEnumerator Wait()
	{
		yield return new WaitForSeconds(1.5f);
		EndAction();
	}

	private void EndAction()
	{
		unitCtrlr.StateMachine.ChangeState(10);
	}

	public void WeaponAim()
	{
		if (unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot] != null && unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.TypeData.IsRange)
		{
			unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Aim();
		}
		if (unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)] != null && unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)].Item.TypeData.IsRange)
		{
			unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)].Aim();
		}
	}

	public void ShootProjectile(int idx)
	{
		if (unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + idx)] != null && unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + idx)].Item.TypeData.IsRange)
		{
			unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + idx)].Shoot(unitCtrlr, targetsPos, targets, noHitCollisions, bodyParts, idx != 0);
		}
	}
}
