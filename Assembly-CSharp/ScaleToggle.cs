using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScaleToggle : MonoBehaviour
{
	public Toggle target;

	public Transform targetTransform;

	public ScaleBlock scale = ScaleBlock.defaultScaleBlock;

	protected Transform Transform
	{
		get
		{
			if (targetTransform == null)
			{
				targetTransform = ((Component)(object)target).transform;
			}
			return targetTransform;
		}
	}

	private void Awake()
	{
		if ((Object)(object)target == null)
		{
			target = GetComponent<Toggle>();
			((UnityEvent<bool>)(object)target.onValueChanged).AddListener((UnityAction<bool>)OnValueChanged);
		}
	}

	private void OnEnable()
	{
		OnValueChanged(isOn: false);
	}

	private void OnValueChanged(bool isOn)
	{
		Scale((!isOn) ? scale.normalScale : scale.highlightedScale);
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
