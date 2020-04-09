using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HighlightButton : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Graphic target;

	private RectTransform _cachedTransform;

	public HightlightAnimate hightlight;

	public bool hideOnExit;

	public bool hideOnSelect;

	private RectTransform cachedTransform
	{
		get
		{
			if (_cachedTransform == null)
			{
				_cachedTransform = GetComponent<RectTransform>();
			}
			return _cachedTransform;
		}
	}

	private void Awake()
	{
		if ((Object)(object)target == null)
		{
			target = GetComponent<Graphic>();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (hideOnSelect)
		{
			hightlight.Deactivate();
		}
		else
		{
			hightlight.Highlight(cachedTransform);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (hideOnExit)
		{
			hightlight.Deactivate();
		}
	}
}
