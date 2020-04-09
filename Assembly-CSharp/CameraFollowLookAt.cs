using UnityEngine;

public class CameraFollowLookAt : CameraBase
{
	private Quaternion targetRotation;

	private Vector3 targetPosition;

	private bool done;

	public void MoveLookAt(Vector3 position, Quaternion rotation)
	{
		targetPosition = position;
		targetRotation = rotation;
		done = false;
	}

	private void Update()
	{
		if (!done)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, 10f * Time.deltaTime);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRotation, 10f * Time.deltaTime);
			done = (base.transform.position == targetPosition && base.transform.rotation == targetRotation);
		}
	}
}
