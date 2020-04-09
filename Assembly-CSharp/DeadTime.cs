using UnityEngine;

public class DeadTime : MonoBehaviour
{
	public float deadTime;

	private void Awake()
	{
		Object.Destroy(base.gameObject, deadTime);
	}
}
