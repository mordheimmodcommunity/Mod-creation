using UnityEngine;

public class RotateAroundCam : ICheapState
{
    private const float MIN_ANGLE = -30f;

    private const float MAX_ANGLE = 50f;

    public Vector3 centerOffset = Vector3.zero;

    private Vector3 lastTargetPosition = Vector3.zero;

    private RaycastHit hit;

    private CameraManager mngr;

    private Transform dummyCam;

    private Transform prevTarget;

    private float previousAngle;

    public float distance;

    public RotateAroundCam(CameraManager camMngr)
    {
        mngr = camMngr;
        dummyCam = camMngr.dummyCam.transform;
        centerOffset = new Vector3(0f, 1.5f, 0f);
        hit = default(RaycastHit);
        prevTarget = null;
    }

    public void Destroy()
    {
    }

    public void Enter(int from)
    {
        Vector3 eulerAngles = dummyCam.rotation.eulerAngles;
        previousAngle = eulerAngles.x;
        if (previousAngle > 180f)
        {
            previousAngle -= 360f;
        }
        previousAngle = Mathf.Clamp(previousAngle, -30f, 50f);
    }

    public void Exit(int to)
    {
    }

    public void Update()
    {
        if (mngr.Target == null)
        {
            return;
        }
        Vector3 vector = mngr.Target.position + centerOffset;
        float num = 0f;
        float num2 = 0f;
        num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
        num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) / 2f;
        num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f;
        num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * 4f;
        Vector3 vector2 = -dummyCam.forward;
        previousAngle = Mathf.Clamp(previousAngle, -30f, 50f);
        float num3 = previousAngle;
        if (num != 0f || num2 != 0f)
        {
            if (num2 > 0f && num2 + num3 > 50f)
            {
                num2 = 50f - num3;
            }
            else if (num2 < 0f && num2 + num3 < -30f)
            {
                num2 = -30f - num3;
            }
            Quaternion lhs = Quaternion.AngleAxis(num, Vector3.up);
            Quaternion rhs = Quaternion.AngleAxis(num2, dummyCam.right);
            vector2 = lhs * rhs * -dummyCam.forward;
        }
        dummyCam.position = vector + vector2 * distance;
        float num4 = 0.2f;
        if (Physics.SphereCast(vector, num4, vector2, out hit, distance, LayerMaskManager.groundMask) && hit.transform != mngr.Target)
        {
            dummyCam.position = hit.point + hit.normal * num4;
        }
        lastTargetPosition = vector;
        Vector3 eulerAngles = dummyCam.rotation.eulerAngles;
        previousAngle = eulerAngles.x;
        if (previousAngle > 180f)
        {
            previousAngle -= 360f;
        }
        if (mngr.LookAtTarget != null)
        {
            dummyCam.LookAt(mngr.LookAtTarget);
        }
        else
        {
            dummyCam.LookAt(vector);
        }
        if (Vector3.SqrMagnitude(mngr.transform.position - dummyCam.position) > 4f)
        {
            mngr.Transition(2f, force: false);
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
