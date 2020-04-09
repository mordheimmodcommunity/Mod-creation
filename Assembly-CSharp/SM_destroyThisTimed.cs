using UnityEngine;

public class SM_destroyThisTimed : MonoBehaviour
{
	public float destroyTime = 5f;

	private void Start()
	{
		Object.Destroy(base.gameObject, destroyTime);
	}
}
