using UnityEngine;

public class SemiConstrainedCamera : ICheapState
{
	private const float MIN_ANGLE = -30f;

	private const float MAX_ANGLE = 50f;

	private const float MAX_ROT_H = 90f;

	public Vector3 centerOffset = Vector3.zero;

	private RaycastHit hit;

	private CameraManager mngr;

	private Transform dummyCam;

	private Transform previousTarget;

	private Transform previousLook;

	private float previousAngle;

	private float oRotH;

	private float rotH;

	public SemiConstrainedCamera(CameraManager camMngr)
	{
		mngr = camMngr;
		dummyCam = camMngr.dummyCam.transform;
		centerOffset = new Vector3(0f, 1.5f, 0f);
		hit = default(RaycastHit);
		previousTarget = null;
	}

	public void Destroy()
	{
	}

	public void Enter(int from)
	{
		if (mngr.Target != previousTarget || mngr.LookAtTarget != previousLook || from != 6)
		{
			previousTarget = mngr.Target;
			previousLook = mngr.LookAtTarget;
			Quaternion quaternion = default(Quaternion);
			if (mngr.LookAtTarget == mngr.Target)
			{
				quaternion = mngr.Target.rotation;
			}
			else
			{
				quaternion.SetLookRotation(mngr.LookAtTarget.position - mngr.Target.position, Vector3.up);
			}
			Vector3 eulerAngles = quaternion.eulerAngles;
			Vector3 position = mngr.Target.position + centerOffset;
			oRotH = eulerAngles.y;
			dummyCam.position = position;
			rotH = -25f;
			dummyCam.rotation = Quaternion.Euler(0f, oRotH + rotH, 0f);
			dummyCam.Translate(-dummyCam.forward * mngr.Zoom, Space.World);
			Vector3 eulerAngles2 = dummyCam.rotation.eulerAngles;
			previousAngle = eulerAngles2.x;
			if (previousAngle > 180f)
			{
				previousAngle -= 360f;
			}
			previousAngle = Mathf.Clamp(previousAngle, -30f, 50f);
		}
	}

	public void Exit(int to)
	{
	}

	public void Update()
	{
		if (!(mngr.Target == null))
		{
			Vector3 vector = mngr.Target.position + centerOffset;
			float num = 0f;
			float num2 = 0f;
			num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
			num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) / 2f;
			num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f;
			num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * 4f;
			float num3 = previousAngle;
			num3 += num2;
			num3 = Mathf.Clamp(num3, -30f, 50f);
			rotH = Mathf.Clamp(rotH + num, -90f, 90f);
			Quaternion lhs = Quaternion.AngleAxis(oRotH + rotH, Vector3.up);
			Quaternion rhs = Quaternion.AngleAxis(num3, Vector3.right);
			dummyCam.rotation = lhs * rhs;
			dummyCam.position = vector + -dummyCam.forward * mngr.Zoom;
			if (Physics.Raycast(vector, -dummyCam.forward, out hit, mngr.Zoom, LayerMaskManager.groundMask) && hit.transform != mngr.Target)
			{
				dummyCam.position = hit.point + dummyCam.forward * 0.1f;
			}
			Vector3 eulerAngles = dummyCam.rotation.eulerAngles;
			previousAngle = eulerAngles.x;
			if (previousAngle > 180f)
			{
				previousAngle -= 360f;
			}
		}
	}

	public void FixedUpdate()
	{
	}

	private Vector3 AdjustLineOfSight(Vector3 newPosition, Vector3 target)
	{
		if (Physics.Linecast(target, newPosition, out RaycastHit hitInfo, 0))
		{
			return hitInfo.point;
		}
		return newPosition;
	}
}
