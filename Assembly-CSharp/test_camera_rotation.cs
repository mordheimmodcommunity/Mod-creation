using UnityEngine;

public class test_camera_rotation : MonoBehaviour
{
	public Transform TargetLookAt;

	public float Distance = 5f;

	public float DistanceMin = 3f;

	public float DistanceMax = 10f;

	private float mouseX;

	private float mouseY;

	private float startingDistance;

	private float desiredDistance;

	public float X_MouseSensitivity = 5f;

	public float Y_MouseSensitivity = 5f;

	public float MouseWheelSensitivity = 5f;

	public float Y_MinLimit = -40f;

	public float Y_MaxLimit = 80f;

	public float DistanceSmooth = 0.05f;

	public float velocityDistance;

	private Vector3 desiredPosition = Vector3.zero;

	public float X_Smooth = 0.05f;

	public float Y_Smooth = 0.1f;

	private float velX;

	private float velY;

	private float velZ;

	private Vector3 position = Vector3.zero;

	private Vector3 direction = Vector3.zero;

	private float rotationX;

	private float rotationY;

	private float distance;

	private void Start()
	{
		Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax);
		startingDistance = Distance;
		Reset();
	}

	private void LateUpdate()
	{
		if (TargetLookAt == null)
		{
			TargetLookAt = GameObject.Find("rig_pelvis").transform;
		}
		HandlePlayerInput();
		CalculateDesiredPosition();
		UpdatePosition();
	}

	private void HandlePlayerInput()
	{
		float num = 0.01f;
		if (Input.GetKey(KeyCode.Mouse1))
		{
			mouseX += PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") * X_MouseSensitivity;
			mouseY -= PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y") * Y_MouseSensitivity;
		}
		mouseY = ClampAngle(mouseY, Y_MinLimit, Y_MaxLimit);
		if (PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_wheel") < 0f - num || PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_wheel") > num)
		{
			desiredDistance = Mathf.Clamp(Distance - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_wheel") * MouseWheelSensitivity, DistanceMin, DistanceMax);
		}
	}

	private float ClampAngle(float angle, float min, float max)
	{
		while (angle < -360f || angle > 360f)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
		}
		return Mathf.Clamp(angle, min, max);
	}

	private void CalculateDesiredPosition()
	{
		Distance = Mathf.SmoothDamp(Distance, desiredDistance, ref velocityDistance, DistanceSmooth);
		desiredPosition = CalculatePosition(mouseY, mouseX, Distance);
	}

	public Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
	{
		direction = new Vector3(0f, 0f, 0f - distance);
		Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0f);
		return TargetLookAt.position + rotation * direction;
	}

	private void UpdatePosition()
	{
		float x = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
		float y = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
		float z = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
		position = new Vector3(x, y, z);
		base.transform.position = position;
		base.transform.LookAt(TargetLookAt);
	}

	private void Reset()
	{
		mouseX = 0f;
		mouseY = 10f;
		Distance = startingDistance;
		desiredDistance = Distance;
	}
}
