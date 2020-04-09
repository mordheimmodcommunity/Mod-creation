using UnityEngine;

public class test_autoRotate : MonoBehaviour
{
	public bool around;

	private void Start()
	{
	}

	private void Update()
	{
		if (!around)
		{
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.y += Time.deltaTime * 180f;
			base.transform.rotation = Quaternion.Euler(eulerAngles);
		}
		else
		{
			base.transform.RotateAround(Vector3.zero, Vector3.up, 40f * Time.deltaTime);
			base.transform.LookAt(new Vector3(0f, 1.5f, 0f));
		}
	}
}
