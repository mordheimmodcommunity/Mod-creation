using UnityEngine;

public class FPSCamera : CameraBase
{
	private const float maxAngle = 30f;

	public Transform shooter;

	public float offsetY;

	public bool invertedAxis;

	public float mouseSpeed = 10f;

	private void LateUpdate()
	{
		if ((bool)shooter)
		{
			Vector3 position = Vector3.zero;
			Quaternion angle = Quaternion.identity;
			GetNextPositionAngle(ref position, ref angle);
			base.transform.position = position;
			float num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") * Time.deltaTime * mouseSpeed;
			float num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) * Time.deltaTime * mouseSpeed * (float)((!invertedAxis) ? 1 : (-1));
			num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * Time.deltaTime;
			num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * Time.deltaTime * (float)((!invertedAxis) ? 1 : (-1));
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			float num3 = eulerAngles.x;
			if (num3 > 180f)
			{
				num3 -= 360f;
			}
			if (num2 > 0f && num2 + num3 > 30f)
			{
				num2 = 30f - num3;
			}
			else if (num2 < 0f && num2 + num3 < -30f)
			{
				num2 = -30f - num3;
			}
			Vector3 eulerAngles2 = base.transform.rotation.eulerAngles;
			float y = eulerAngles2.y;
			Vector3 eulerAngles3 = shooter.rotation.eulerAngles;
			num3 = (y - eulerAngles3.y) % 360f;
			if (num3 > 180f)
			{
				num3 -= 360f;
			}
			else if (num3 < -180f)
			{
				num3 += 360f;
			}
			num3 += num;
			if (num3 > 30f)
			{
				num3 = 30f;
			}
			else if (num3 < -30f)
			{
				num3 = -30f;
			}
			Transform transform = base.transform;
			Vector3 eulerAngles4 = base.transform.rotation.eulerAngles;
			float x = eulerAngles4.x + num2;
			Vector3 eulerAngles5 = shooter.rotation.eulerAngles;
			float y2 = eulerAngles5.y + num3;
			Vector3 eulerAngles6 = base.transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(x, y2, eulerAngles6.z);
		}
	}

	public override void GetNextPositionAngle(ref Vector3 position, ref Quaternion angle)
	{
		if ((bool)shooter)
		{
			position = shooter.position + Vector3.up * offsetY;
			angle = shooter.rotation;
		}
		else
		{
			position = base.transform.position;
			angle = base.transform.rotation;
		}
	}

	public override void SetTarget(Transform target)
	{
		shooter = target;
	}

	public override Transform GetTarget()
	{
		return shooter;
	}
}
