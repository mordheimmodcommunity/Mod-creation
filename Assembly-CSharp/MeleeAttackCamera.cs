using UnityEngine;

public class MeleeAttackCamera : ICheapState
{
    private const float MIN_ANGLE = 25f;

    private const float MAX_ANGLE = 25f;

    private const float MAX_ROT_H = 25f;

    public Vector3 centerOffset = Vector3.zero;

    private RaycastHit hit;

    private CameraManager mngr;

    private Transform dummyCam;

    private Vector3 lastTargetPosition = Vector3.zero;

    private bool isSnapping;

    private float snappingTime;

    private float oRotH;

    private float oRotV;

    public MeleeAttackCamera(CameraManager camMngr)
    {
        mngr = camMngr;
        dummyCam = camMngr.dummyCam.transform;
        centerOffset = new Vector3(0f, 1.5f, 0f);
        hit = default(RaycastHit);
    }

    public void Destroy()
    {
    }

    public void Enter(int from)
    {
        snappingTime = 0f;
        isSnapping = true;
        Quaternion quaternion = default(Quaternion);
        quaternion.SetLookRotation(mngr.LookAtTarget.position - mngr.Target.position, Vector3.up);
        Vector3 eulerAngles = quaternion.eulerAngles;
        Vector3 vector = mngr.Target.position + (mngr.LookAtTarget.position - mngr.Target.position) / 2f + centerOffset;
        oRotH = eulerAngles.y;
        oRotV = eulerAngles.x;
        dummyCam.rotation = Quaternion.Euler(oRotV, oRotH - 110f, 0f);
        dummyCam.position = vector;
        dummyCam.transform.Translate(-dummyCam.transform.forward * 3f, Space.World);
        dummyCam.LookAt(vector - Vector3.up * 0.5f);
        mngr.Transition(10f);
    }

    public void Exit(int to)
    {
    }

    public void Update()
    {
        Vector3 vector = mngr.Target.position + centerOffset;
        if (!isSnapping)
        {
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
        float num = oRotH - 90f;
        Vector3 eulerAngles = dummyCam.eulerAngles;
        float y = eulerAngles.y;
        snappingTime += Time.deltaTime;
        dummyCam.rotation = Quaternion.Slerp(dummyCam.rotation, Quaternion.Euler(15f, num, 0f), snappingTime / 7.5f);
        Vector3 position2 = targetCenter;
        position2 += dummyCam.rotation * Vector3.back * mngr.Zoom;
        dummyCam.position = position2;
        Vector3 eulerAngles2 = dummyCam.eulerAngles;
        if (Mathf.Abs(Mathf.DeltaAngle(eulerAngles2.y, num)) < 2f)
        {
            isSnapping = false;
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
