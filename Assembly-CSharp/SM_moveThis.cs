using UnityEngine;

public class SM_moveThis : MonoBehaviour
{
	public float translationSpeedX;

	public float translationSpeedY = 1f;

	public float translationSpeedZ;

	public bool local = true;

	private void Update()
	{
		if (local)
		{
			base.transform.Translate(new Vector3(translationSpeedX, translationSpeedY, translationSpeedZ) * Time.deltaTime);
		}
		else
		{
			base.transform.Translate(new Vector3(translationSpeedX, translationSpeedY, translationSpeedZ) * Time.deltaTime, Space.World);
		}
	}
}
