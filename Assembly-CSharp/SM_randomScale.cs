using UnityEngine;

public class SM_randomScale : MonoBehaviour
{
	public float minScale = 1f;

	public float maxScale = 2f;

	private void Start()
	{
		float num = Random.Range(minScale, maxScale);
		base.transform.localScale = new Vector3(num, num, num);
	}
}
