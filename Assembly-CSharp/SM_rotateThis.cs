using UnityEngine;

public class SM_rotateThis : MonoBehaviour
{
	public float rotationSpeedX = 90f;

	public float rotationSpeedY;

	public float rotationSpeedZ;

	public bool local = true;

	private void Update()
	{
		if (local)
		{
			base.transform.Rotate(new Vector3(rotationSpeedX, rotationSpeedY, rotationSpeedZ) * Time.deltaTime);
		}
		else
		{
			base.transform.Rotate(new Vector3(rotationSpeedX, rotationSpeedY, rotationSpeedZ) * Time.deltaTime, Space.World);
		}
	}
}
