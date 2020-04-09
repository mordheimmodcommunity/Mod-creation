using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ConsoleSafeCanvasSize : MonoBehaviour
{
	private Canvas canvas;

	private float lastScaleFactor;

	private float lastSafeAreaRatio;

	private RectTransform rt;

	private void Start()
	{
		canvas = GetComponent<Canvas>();
		GameObject gameObject = new GameObject("safe_zone", typeof(RectTransform));
		rt = (RectTransform)gameObject.transform;
		rt.SetParent(canvas.transform);
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.localScale = Vector3.one;
		for (int num = canvas.transform.childCount - 1; num >= 0; num--)
		{
			Transform child = canvas.transform.GetChild(num);
			if (child != rt)
			{
				child.SetParent(rt, worldPositionStays: true);
				child.SetAsFirstSibling();
			}
		}
		Update();
	}

	private void Update()
	{
		float num = lastScaleFactor;
		num = Mathf.Lerp(0.9f, 1f, PandoraSingleton<GameManager>.Instance.Options.graphicsGuiScale);
		if (num != lastScaleFactor)
		{
			lastScaleFactor = num;
			rt.localScale = new Vector3(lastScaleFactor, lastScaleFactor, 1f);
		}
	}
}
