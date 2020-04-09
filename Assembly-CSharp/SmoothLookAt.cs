using UnityEngine;

public class SmoothLookAt : CameraBase
{
	public float damping = 6f;

	public bool smooth = true;

	public float minDistance = 10f;

	public float maxDistance = 50f;

	public float scrollSpeed = 10f;

	private Ray ray;

	private Plane planY;

	private void Start()
	{
		ray = new Ray(Vector3.zero, Vector3.zero);
		planY = new Plane(Vector3.up, Vector3.zero);
	}

	private void Update()
	{
		Vector3 vector = Vector3.zero;
		if ((bool)target)
		{
			vector = target.position;
		}
		else
		{
			ray.origin = base.transform.position;
			ray.direction = base.transform.rotation * Vector3.forward;
			float enter = 0f;
			if (planY.Raycast(ray, out enter))
			{
				vector = base.transform.position + ray.direction.normalized * enter;
			}
		}
		float num = Vector3.Distance(vector, base.transform.position);
		num -= PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_wheel") * scrollSpeed;
		if (num < minDistance)
		{
			num = minDistance;
		}
		else if (num > maxDistance)
		{
			num = maxDistance;
		}
		Vector3 point = base.transform.position - vector;
		point.Normalize();
		Quaternion rotation = Quaternion.AngleAxis(PandoraSingleton<PandoraInput>.Instance.GetAxis("h"), Vector3.up);
		point = rotation * point;
		base.transform.position = vector + point * num;
		if (smooth)
		{
			Quaternion b = Quaternion.LookRotation(vector - base.transform.position);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * damping);
		}
		else
		{
			base.transform.LookAt(vector);
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
