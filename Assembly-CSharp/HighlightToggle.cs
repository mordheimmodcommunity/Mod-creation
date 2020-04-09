using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class HighlightToggle : MonoBehaviour
{
	[FormerlySerializedAs("target")]
	public Toggle _target;

	public RectTransform _targetTransform;

	[FormerlySerializedAs("hightlight")]
	public HightlightAnimate _hightlight;

	public bool hideOnExit;

	public bool hideOnSelect;

	public bool findInParent;

	public Toggle target
	{
		get
		{
			if ((Object)(object)_target == null)
			{
				_target = GetComponent<Toggle>();
			}
			return _target;
		}
	}

	private RectTransform targetTransform
	{
		get
		{
			if (_targetTransform == null)
			{
				_targetTransform = (RectTransform)base.transform;
			}
			return _targetTransform;
		}
	}

	public HightlightAnimate hightlight
	{
		get
		{
			if (_hightlight == null && findInParent && base.transform.parent != null)
			{
				_hightlight = base.transform.parent.GetComponentsInChildren<HightlightAnimate>(includeInactive: true)[0];
			}
			return _hightlight;
		}
	}

	private void Awake()
	{
		((UnityEvent<bool>)(object)target.onValueChanged).AddListener((UnityAction<bool>)OnValueChanged);
	}

	public void OnValueChanged(bool isOn)
	{
		if (isOn)
		{
			if (hideOnSelect)
			{
				hightlight.Deactivate();
			}
			else
			{
				hightlight.Highlight(targetTransform);
			}
		}
		else if (hideOnExit)
		{
			hightlight.Deactivate();
		}
	}
}
