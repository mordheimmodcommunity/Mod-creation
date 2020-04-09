using UnityEngine;

public class LoopCredits : MonoBehaviour
{
	private RectTransform canvasRect;

	private RectTransform creditsRect;

	private void Awake()
	{
		canvasRect = (RectTransform)GetComponentInParent<Canvas>().transform;
		creditsRect = (RectTransform)base.transform;
	}

	private void Update()
	{
		Vector3[] array = new Vector3[4];
		canvasRect.GetWorldCorners(array);
		float y = array[1].y;
		creditsRect.GetWorldCorners(array);
		float y2 = array[0].y;
		if (y2 > y)
		{
			Transform transform = base.transform;
			Vector3 position = base.transform.position;
			float x = position.x;
			Vector3 position2 = base.transform.position;
			transform.position = new Vector3(x, 0f, position2.z);
		}
	}
}
