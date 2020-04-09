using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectEnsureVisible : MonoBehaviour
{
	public float AnimTime = 0.15f;

	public bool Snap;

	public RectTransform MaskTransform;

	private ScrollRect _mScrollRect;

	private RectTransform _mScrollTransform;

	private RectTransform _mContent;

	private void Awake()
	{
		_mScrollRect = GetComponent<ScrollRect>();
		_mScrollTransform = (((Component)(object)_mScrollRect).transform as RectTransform);
		_mContent = _mScrollRect.get_content();
	}

	public void CenterOnItem(RectTransform target)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPointInWidget = GetWorldPointInWidget(_mScrollTransform, GetWidgetWorldPoint(target));
		Vector3 worldPointInWidget2 = GetWorldPointInWidget(_mScrollTransform, GetWidgetWorldPoint(MaskTransform));
		Vector3 vector = worldPointInWidget2 - worldPointInWidget;
		vector.z = 0f;
		if (!_mScrollRect.get_horizontal())
		{
			vector.x = 0f;
		}
		if (!_mScrollRect.get_vertical())
		{
			vector.y = 0f;
		}
		Vector2 vector2 = _mContent.rect.size - _mScrollTransform.rect.size;
		Vector2 b = new Vector2(Mathf.Approximately(vector2.x, 0f) ? 0f : (vector.x / vector2.x), Mathf.Approximately(vector2.y, 0f) ? 0f : (vector.y / vector2.y));
		Vector2 normalizedPosition = _mScrollRect.get_normalizedPosition() - b;
		if ((int)_mScrollRect.get_movementType() != 0)
		{
			normalizedPosition.x = Mathf.Clamp01(normalizedPosition.x);
			normalizedPosition.y = Mathf.Clamp01(normalizedPosition.y);
		}
		_mScrollRect.set_normalizedPosition(normalizedPosition);
	}

	private Vector3 GetWidgetWorldPoint(RectTransform target)
	{
		Vector2 pivot = target.pivot;
		float num = 0.5f - pivot.x;
		Vector2 size = target.rect.size;
		float x = num * size.x;
		Vector2 pivot2 = target.pivot;
		float num2 = 0.5f - pivot2.y;
		Vector2 size2 = target.rect.size;
		Vector3 b = new Vector3(x, num2 * size2.y, 0f);
		Vector3 position = target.localPosition + b;
		return target.parent.TransformPoint(position);
	}

	private Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
	{
		return target.InverseTransformPoint(worldPoint);
	}
}
