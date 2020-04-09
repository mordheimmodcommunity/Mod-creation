using Prometheus;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
public class Projectile : MonoBehaviour
{
	private enum CamState
	{
		NONE,
		LOOKTARGET,
		ANIMATE
	}

	public float speed = 10f;

	public AudioClip hitFailSound;

	public AudioClip missSound;

	public bool destroyOnArrival;

	public string fxHitObstacle;

	public bool randPenetration;

	private bool launched;

	private Vector3 startPos;

	private Vector3 previousPos;

	private float maxDistance;

	private Vector3 targetPos;

	private AttackResultId attackResult;

	private UnitController attackerCtrlr;

	private MonoBehaviour target;

	private bool missSoundPlayed;

	private bool noHitCollision;

	private bool isSecondary;

	private Transform bodyPart;

	private MoveController moveCtrlr;

	private List<TrailRenderer> trails;

	private AudioSource audioSource;

	private void Awake()
	{
		moveCtrlr = GetComponent<MoveController>();
		trails = new List<TrailRenderer>(GetComponentsInChildren<TrailRenderer>());
		ActivateTrails(active: false);
		audioSource = GetComponent<AudioSource>();
	}

	private void FixedUpdate()
	{
		if (!launched)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(base.transform.position - startPos);
		base.transform.LookAt(base.transform.position + (base.transform.position - previousPos));
		previousPos = base.transform.position;
		if (!missSoundPlayed && num > (maxDistance - 2f) * (maxDistance - 2f))
		{
			if (noHitCollision)
			{
				missSoundPlayed = true;
				if (missSound != null && audioSource != null)
				{
					audioSource.clip = missSound;
					audioSource.Play();
				}
			}
			if (attackResult == AttackResultId.MISS)
			{
				DisplayFlyingText();
			}
		}
		if (!(num >= maxDistance * maxDistance))
		{
			return;
		}
		if (noHitCollision)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (attackResult >= AttackResultId.HIT_NO_WOUND)
		{
			if (target is UnitController)
			{
				UnitController unitController = (UnitController)target;
				base.transform.SetParent(bodyPart);
				base.transform.localPosition = Vector3.zero;
				if (randPenetration)
				{
					base.transform.position += base.transform.forward * (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0.10000000149011612, 0.5);
				}
				Quaternion identity = Quaternion.identity;
				identity.SetLookRotation(base.transform.rotation.eulerAngles, base.transform.forward * -1f);
				unitController.SetHitData(bodyPart, identity);
				if (!isSecondary)
				{
					Vector3 forward = base.transform.forward;
					forward.y = 0f;
					Vector3 forward2 = target.transform.forward;
					forward2.y = 0f;
					int num2 = (unitController.unit.Status != UnitStateId.OUT_OF_ACTION) ? 1 : 0;
					num2 += ((!(Vector3.Angle(forward, forward2) > 90f)) ? 1 : 0);
					unitController.PlayDefState(attackResult, num2, unitController.unit.Status);
					if (attackerCtrlr.CurrentAction.fxData != null && !string.IsNullOrEmpty(attackerCtrlr.CurrentAction.fxData.ImpactFx))
					{
						PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(attackerCtrlr.CurrentAction.fxData.ImpactFx, unitController, null, null);
					}
				}
			}
			else if (target is Destructible)
			{
				((Destructible)target).projectiles.Add(this);
				if (!isSecondary)
				{
					((Destructible)target).Hit(attackerCtrlr);
				}
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(fxHitObstacle))
			{
				PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(fxHitObstacle, base.transform.position);
			}
			if (hitFailSound != null && audioSource != null)
			{
				audioSource.clip = hitFailSound;
				audioSource.Play();
			}
			destroyOnArrival = true;
		}
		moveCtrlr.StopMoving();
		ActivateTrails(active: false);
		base.enabled = false;
		if (destroyOnArrival)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void DisplayFlyingText()
	{
		UnitController defenderCtrlr = (UnitController)target;
		PandoraSingleton<FlyingTextManager>.Instance.GetFlyingText(FlyingTextId.ACTION, delegate(FlyingText fl)
		{
			((FlyingLabel)fl).Play(defenderCtrlr.BonesTr[BoneId.RIG_HEAD].position, false, defenderCtrlr.flyingLabel);
		});
	}

	public void Launch(Vector3 pos, UnitController attacker, MonoBehaviour target, bool noCollision, Transform part, bool secondary)
	{
		launched = true;
		missSoundPlayed = false;
		attackerCtrlr = attacker;
		this.target = target;
		if (target is UnitController)
		{
			attackResult = ((UnitController)target).attackResultId;
		}
		else if (target is Destructible)
		{
			attackResult = AttackResultId.HIT;
		}
		noHitCollision = noCollision;
		bodyPart = part;
		isSecondary = secondary;
		base.transform.SetParent(null);
		startPos = base.transform.position;
		previousPos = base.transform.position;
		if (noCollision)
		{
			pos += Vector3.Normalize(pos - startPos) * 2f;
		}
		targetPos = pos;
		maxDistance = Vector3.Distance(targetPos, startPos);
		base.transform.LookAt(targetPos);
		moveCtrlr.StartMoving(base.transform.forward, speed);
		ActivateTrails(active: true);
	}

	private void ActivateTrails(bool active)
	{
		if (trails != null && trails.Count > 0)
		{
			for (int i = 0; i < trails.Count; i++)
			{
				TrailRenderer trailRenderer = trails[i];
				trailRenderer.gameObject.SetActive(active);
			}
		}
	}
}
