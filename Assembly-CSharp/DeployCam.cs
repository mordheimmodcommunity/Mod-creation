using UnityEngine;

public class DeployCam : ICheapState
{
    private const float MIN_ANGLE = 10f;

    private const float MAX_ANGLE = 75f;

    private bool isSnapping;

    public Vector3 centerOffset = Vector3.zero;

    private Vector3 lastTargetPosition = Vector3.zero;

    private RaycastHit hit;

    private CameraManager mngr;

    private Transform dummyCam;

    private Transform prevTarget;

    private float snappingTime;

    private float previousAngle;

    public DeployCam(CameraManager camMngr)
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
        mngr.SetZoomLevel(2u);
        if (prevTarget != mngr.Target)
        {
            prevTarget = mngr.Target;
            Vector3 position = mngr.Target.position + centerOffset;
            dummyCam.position = position;
            Transform transform = dummyCam;
            Vector3 eulerAngles = mngr.Target.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, eulerAngles.y - 90f, 0f);
            dummyCam.Translate(-dummyCam.forward * mngr.Zoom * 3f, Space.World);
        }
        snappingTime = 0f;
        isSnapping = true;
        Vector3 eulerAngles2 = dummyCam.rotation.eulerAngles;
        previousAngle = eulerAngles2.x;
        if (previousAngle > 180f)
        {
            previousAngle -= 360f;
        }
        previousAngle = Mathf.Clamp(previousAngle, 10f, 75f);
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
        if (isSnapping)
        {
            ApplySnapping(vector);
        }
        float num = 0f;
        float num2 = 0f;
        num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
        num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) / 2f;
        num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f;
        num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * 4f;
        Vector3 vector2 = -dummyCam.forward;
        previousAngle = Mathf.Clamp(previousAngle, 10f, 75f);
        float num3 = previousAngle;
        if (num != 0f || num2 != 0f)
        {
            if (num2 > 0f && num2 + num3 > 75f)
            {
                num2 = 75f - num3;
            }
            else if (num2 < 0f && num2 + num3 < 10f)
            {
                num2 = 10f - num3;
            }
            isSnapping = false;
            Quaternion lhs = Quaternion.AngleAxis(num, Vector3.up);
            Quaternion rhs = Quaternion.AngleAxis(num2, dummyCam.right);
            vector2 = lhs * rhs * -dummyCam.forward;
        }
        if (!isSnapping)
        {
            dummyCam.position = vector + vector2 * mngr.Zoom * 3f;
        }
        float num4 = 0.2f;
        if (Physics.SphereCast(vector, num4, vector2, out hit, mngr.Zoom * 3f, LayerMaskManager.groundMask) && hit.transform != mngr.Target)
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
    }

    public void FixedUpdate()
    {
    }

    private void ApplySnapping(Vector3 targetCenter)
    {
        Vector3 position = dummyCam.position;
        Vector3 vector = position - targetCenter;
        vector.y = 0f;
        Vector3 eulerAngles = mngr.Target.eulerAngles;
        float y = eulerAngles.y;
        Vector3 eulerAngles2 = dummyCam.eulerAngles;
        float y2 = eulerAngles2.y;
        snappingTime += Time.deltaTime;
        dummyCam.rotation = Quaternion.Slerp(dummyCam.rotation, Quaternion.Euler(15f, y, 0f), snappingTime / 7.5f);
        Vector3 position2 = targetCenter;
        position2 += dummyCam.rotation * Vector3.back * mngr.Zoom;
        dummyCam.position = position2;
        Vector3 eulerAngles3 = dummyCam.eulerAngles;
        if (Mathf.Abs(Mathf.DeltaAngle(eulerAngles3.y, y)) < 2f)
        {
            Vector3 eulerAngles4 = dummyCam.eulerAngles;
            if (Mathf.Abs(Mathf.DeltaAngle(eulerAngles4.x, 15f)) < 2f)
            {
                isSnapping = false;
            }
        }
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
