using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScaleButton : MonoBehaviour, IPointerDownHandler, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Transform target;

	public ScaleBlock scale = ScaleBlock.defaultScaleBlock;

	protected Transform Transform
	{
		get
		{
			if (target == null)
			{
				target = base.transform;
			}
			return target;
		}
	}

	void IDeselectHandler.OnDeselect(BaseEventData eventData)
	{
		Scale(scale.normalScale);
	}

	private void OnEnable()
	{
		Scale(scale.normalScale);
	}

	private void OnDisable()
	{
		Scale(scale.disabledScale);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Scale(scale.pressedScale);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Scale(scale.highlightedScale);
	}

	private void Scale(Vector2 newScale)
	{
		float x = newScale.x;
		float y = newScale.y;
		Vector3 localScale = Transform.localScale;
		Vector3 vector = new Vector3(x, y, localScale.z);
		if (Application.isPlaying)
		{
			ShortcutExtensions.DOScale(Transform, vector, scale.duration);
		}
	}
}
