using Prometheus;
using UnityEngine;

public class ApolloObjectRotate : MonoBehaviour
{
	public Vector3 RotationPerSecond = new Vector3(0f, 360f, 0f);

	private Transform[] objectTrans;

	private void Start()
	{
		objectTrans = GetComponentsInChildren<Transform>(includeInactive: true);
		CameraManager component = Camera.main.gameObject.GetComponent<CameraManager>();
		component.SetDOFTarget(base.transform, 0.5f);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Rotate rotate = base.transform.GetChild(i).gameObject.AddComponent<Rotate>();
			rotate.useWorldSpace = false;
			rotate.rotPerSec = RotationPerSecond;
		}
	}
}
