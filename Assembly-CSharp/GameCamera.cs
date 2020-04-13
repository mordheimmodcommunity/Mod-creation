using UnityEngine;

public class GameCamera : CameraBase
{
    public Vector3 targetPosition = Vector3.zero;

    public float damping = 6f;

    public bool smooth;

    public float minDistance = 5f;

    public float maxDistance = 30f;

    public float moveSpeed = 10f;

    public float scrollSpeed = 5f;

    private float currentDistance;

    private Vector3 offsetVector = new Vector3(0f, 1f, 0f);

    private void Start()
    {
        currentDistance = (maxDistance - minDistance) / 2f;
    }

    private void Update()
    {
        currentDistance -= PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_wheel") * scrollSpeed;
        if (currentDistance < minDistance)
        {
            currentDistance = minDistance;
        }
        else if (currentDistance > maxDistance)
        {
            currentDistance = maxDistance;
        }
        float axisRaw = PandoraSingleton<PandoraInput>.Instance.GetAxisRaw("h");
        float axisRaw2 = PandoraSingleton<PandoraInput>.Instance.GetAxisRaw("v");
        if (axisRaw != 0f || axisRaw2 != 0f)
        {
            Vector3 forward = base.transform.forward;
            forward.y = 0f;
            Vector3 a = axisRaw * base.transform.right + axisRaw2 * forward;
            a.Normalize();
            base.transform.position += a * Time.deltaTime * moveSpeed;
            targetPosition += a * Time.deltaTime * moveSpeed;
        }
        Vector3 point = base.transform.position - targetPosition;
        point.Normalize();
        Quaternion rotation = Quaternion.AngleAxis(PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x"), Vector3.up);
        point = rotation * point;
        base.transform.position = targetPosition + point * currentDistance;
        if (smooth)
        {
            Quaternion b = Quaternion.LookRotation(targetPosition - base.transform.position);
            base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * damping);
        }
        else
        {
            base.transform.LookAt(targetPosition);
        }
    }

    public override void GetNextPositionAngle(ref Vector3 position, ref Quaternion angle)
    {
        position = base.transform.position;
        angle = Quaternion.LookRotation(targetPosition - base.transform.position);
    }

    public override void SetTarget(Transform target)
    {
        if (target != null)
        {
            targetPosition = target.position + offsetVector;
        }
    }
}
