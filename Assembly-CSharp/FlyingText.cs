using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class FlyingText : MonoBehaviour
{
	public Color startColor;

	public Color endColor;

	public Vector3 startOffset;

	public Vector3 movement;

	public float duration;

	public float clampBorderSize = 0.05f;

	[HideInInspector]
	public Vector3 moveOffset;

	[HideInInspector]
	public Vector3 startPosition;

	public bool clamped;

	private Transform anchor;

	public bool Done
	{
		get;
		protected set;
	}

	private void Awake()
	{
		Done = false;
	}

	private void OnDestroy()
	{
		Destroy();
	}

	public virtual void Destroy()
	{
		DOTween.Kill((object)this, false);
	}

	public void Play(Vector3 position, Transform anchorTr = null)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		moveOffset = Vector3.zero;
		startPosition = position;
		anchor = anchorTr;
		Done = false;
		if (duration > 0f)
		{
			TweenParams val = (TweenParams)(object)new TweenParams();
			val.OnComplete((TweenCallback)(object)new TweenCallback(Deactivate));
			TweenSettingsExtensions.SetAs<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => moveOffset), (DOSetter<Vector3>)delegate(Vector3 pos)
			{
				moveOffset = pos;
			}, movement, duration), val);
		}
	}

	protected virtual void LateUpdate()
	{
		if (!Done)
		{
			Vector3 localPosition = Camera.main.WorldToScreenPoint(((!(anchor != null)) ? Vector3.zero : anchor.position) + startPosition + startOffset + moveOffset);
			localPosition.z = 0f;
			if (clamped)
			{
				float num = (base.transform as RectTransform).rect.width / 2f;
				float num2 = (base.transform as RectTransform).rect.height / 2f;
				localPosition.x = Mathf.Clamp(localPosition.x, PandoraSingleton<FlyingTextManager>.Instance.canvasCorners[0].x + num, PandoraSingleton<FlyingTextManager>.Instance.canvasCorners[2].x - num);
				localPosition.y = Mathf.Clamp(localPosition.y, PandoraSingleton<FlyingTextManager>.Instance.canvasCorners[0].y + num2, PandoraSingleton<FlyingTextManager>.Instance.canvasCorners[2].y - num2);
			}
			base.transform.localPosition = localPosition;
		}
	}

	public virtual void Deactivate()
	{
		Done = true;
		base.gameObject.SetActive(value: false);
	}
}
