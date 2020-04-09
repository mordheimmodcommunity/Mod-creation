using UnityEngine;

public class SmoothFollow : CameraBase
{
	public float damping = 1f;

	public bool fixedCam = true;

	public float minDistance = 10f;

	public float maxDistance = 50f;

	private Vector3 startingDirection;

	private float currentDistance;

	private Quaternion startingRotation;

	private Quaternion targetStartRotation;

	private Vector3 cameraSpeed = Vector3.zero;

	private void Start()
	{
		if ((bool)target)
		{
			startingDirection = target.transform.position - base.transform.position;
			currentDistance = Vector3.Distance(Vector3.zero, startingDirection);
			startingDirection.Normalize();
			startingRotation = base.transform.rotation;
			targetStartRotation = Quaternion.Inverse(target.transform.rotation);
			base.gameObject.layer = 2;
		}
	}

	private void Update()
	{
		if ((bool)target)
		{
			currentDistance -= PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_wheel");
			if (currentDistance < minDistance)
			{
				currentDistance = minDistance;
			}
			else if (currentDistance > maxDistance)
			{
				currentDistance = maxDistance;
			}
			Vector3 vector = startingDirection * currentDistance;
			if (!fixedCam)
			{
				Quaternion quaternion = target.transform.rotation * targetStartRotation;
				base.transform.rotation = quaternion * startingRotation;
				vector = quaternion * vector;
			}
			else
			{
				base.transform.rotation = startingRotation;
			}
			Vector3 vector2 = target.transform.position - vector;
			Vector3 position = target.transform.position;
			float num = Vector3.Distance(vector2, position);
			if (Physics.Raycast(position, vector2 - position, out RaycastHit hitInfo, num + 1f))
			{
				vector2 = hitInfo.point;
			}
			base.transform.position = Vector3.SmoothDamp(base.transform.position, vector2, ref cameraSpeed, damping * Time.deltaTime);
		}
	}

	public override void GetNextPositionAngle(ref Vector3 position, ref Quaternion angle)
	{
		position = base.transform.position;
		if ((bool)target)
		{
			angle = Quaternion.LookRotation(target.position - base.transform.position);
		}
		else
		{
			angle = base.transform.rotation;
		}
	}
}
