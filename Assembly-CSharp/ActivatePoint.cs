using Pathfinding;
using UnityEngine;

public class ActivatePoint : InteractivePoint
{
	public GameObject alternateVisual;

	private Dissolver alternateDissolver;

	public bool reverse;

	public bool activated;

	public bool mirrorAnim;

	public bool destroy;

	public bool destroyOnlyTriggers;

	public bool consumeRequestedItem;

	private OccupationChecker occupation;

	private Animation[] animations;

	private NavmeshCut cutter;

	public override void Init(uint id)
	{
		base.Init(id);
		animations = GetComponentsInChildren<Animation>(includeInactive: true);
		for (int i = 0; i < animations.Length; i++)
		{
			animations[i].Stop();
		}
		cutter = GetComponentInChildren<NavmeshCut>();
		occupation = GetComponentInChildren<OccupationChecker>();
		if (alternateVisual != null)
		{
			alternateDissolver = alternateVisual.AddComponent<Dissolver>();
			alternateDissolver.dissolveSpeed = apparitionDelay;
			alternateDissolver.Hide(hide: true, force: true);
		}
		RefreshAnims(force: true);
	}

	public virtual void Activate(UnitController unitCtrlr, bool force = false)
	{
		activated = !activated;
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateActivated(guid, activated);
		if (!reverse)
		{
			if (visual != null && alternateVisual != null)
			{
				visualDissolver.Hide(hide: true);
				alternateDissolver.Hide(hide: false);
			}
			if (destroy)
			{
				DestroyVisual(destroyOnlyTriggers);
			}
			else
			{
				SetTriggerVisual();
			}
		}
		SpawnFxs(activated);
		if (cutter != null)
		{
			cutter.ForceUpdate();
		}
		RefreshAnims(force);
	}

	public bool IsAnimationPlaying()
	{
		if (animations != null)
		{
			for (int i = 0; i < animations.Length; i++)
			{
				if (animations[i] != null && animations[i].isPlaying)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void SetTriggerVisual()
	{
		SetTriggerVisual(!activated);
	}

	public void RefreshAnims(bool force = false)
	{
		for (int i = 0; i < animations.Length; i++)
		{
			Animation animation = animations[i];
			if ((bool)animation && animation.clip != null)
			{
				animation.Stop();
				animation.wrapMode = WrapMode.Default;
				animation.cullingType = AnimationCullingType.AlwaysAnimate;
				float num = (!activated) ? 1 : 0;
				num = ((!mirrorAnim) ? num : ((num + 1f) % 2f));
				num = ((!force) ? num : ((num + 1f) % 2f));
				float num2 = activated ? 1 : (-1);
				num2 *= (float)((!mirrorAnim) ? 1 : (-1));
				animation[animation.clip.name].normalizedTime = num;
				animation[animation.clip.name].speed = num2;
				animation.Play(animation.clip.name);
			}
		}
	}

	protected override bool LinkValid(UnitController unitCtrlr, bool reverseCondition)
	{
		return reverse || activated;
	}

	protected override bool CanInteract(UnitController unitCtrlr)
	{
		return (occupation == null || occupation.Occupation == 0) && (reverse || !activated) && base.CanInteract(unitCtrlr) && !unitCtrlr.unit.BothArmsMutated() && unitCtrlr.unit.Data.UnitSizeId != UnitSizeId.LARGE && !IsAnimationPlaying();
	}
}
