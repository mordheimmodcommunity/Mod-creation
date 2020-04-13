using UnityEngine;

public class CharacterFollowCamHack : CameraBase
{
    public float minDistance = 3f;

    public float maxDistance = 7f;

    public float minAngle = -10f;

    public float maxAngle = 45f;

    public float distance = 4f;

    public float height = 1f;

    public float smoothLag = 0.2f;

    public float maxSpeed = 10f;

    public float snapLag = 0.3f;

    public float clampHeadPositionScreenSpace = 0.75f;

    private bool isSnapping;

    private Vector3 headOffset = Vector3.zero;

    private Vector3 centerOffset = Vector3.zero;

    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        centerOffset = new Vector3(0f, 1f, 0f);
        headOffset = new Vector3(0f, 2f, 0f);
    }

    private void Start()
    {
        isSnapping = true;
    }

    private void LateUpdate()
    {
        if (!(target == null))
        {
            distance -= PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_wheel");
            if (distance < minDistance)
            {
                distance = minDistance;
            }
            else if (distance > maxDistance)
            {
                distance = maxDistance;
            }
            Vector3 vector = target.position + centerOffset;
            if (isSnapping)
            {
                ApplySnapping(vector);
            }
            else
            {
                float x = vector.x;
                Vector3 position = base.transform.position;
                ApplyPositionDamping(new Vector3(x, position.y, vector.z));
            }
            Vector3 point = base.transform.rotation * Vector3.back;
            point.Normalize();
            float num = 0f;
            float num2 = 0f;
            if (PandoraSingleton<PandoraInput>.Instance.GetKey("action"))
            {
                num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
                num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) / 2f;
                num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 10f;
                num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * 10f;
            }
            Vector3 eulerAngles = base.transform.rotation.eulerAngles;
            float num3 = eulerAngles.x;
            if (num3 > 180f)
            {
                num3 -= 360f;
            }
            if (num2 > 0f && num2 + num3 > maxAngle)
            {
                num2 = maxAngle - num3;
            }
            else if (num2 < 0f && num2 + num3 < minAngle)
            {
                num2 = minAngle - num3;
            }
            if (num != 0f || num2 != 0f)
            {
                isSnapping = false;
                Quaternion lhs = Quaternion.AngleAxis(num, Vector3.up);
                Quaternion rhs = Quaternion.AngleAxis(num2, base.transform.right);
                point = lhs * rhs * point;
                base.transform.position = vector + point * distance;
            }
            base.transform.LookAt(vector);
        }
    }

    private void ApplySnapping(Vector3 targetCenter)
    {
        Vector3 position = base.transform.position;
        Vector3 vector = position - targetCenter;
        vector.y = 0f;
        float magnitude = vector.magnitude;
        Vector3 eulerAngles = target.eulerAngles;
        float y = eulerAngles.y;
        Vector3 eulerAngles2 = base.transform.eulerAngles;
        float y2 = eulerAngles2.y;
        y2 = Mathf.SmoothDampAngle(y2, y, ref velocity.x, snapLag);
        magnitude = Mathf.SmoothDamp(magnitude, distance, ref velocity.z, snapLag);
        Vector3 vector2 = targetCenter;
        vector2 += Quaternion.Euler(0f, y2, 0f) * Vector3.back * magnitude;
        vector2.y = Mathf.SmoothDamp(position.y, targetCenter.y + height, ref velocity.y, smoothLag, maxSpeed);
        vector2 = AdjustLineOfSight(vector2, targetCenter);
        base.transform.position = vector2;
        if (AngleDistance(y2, y) < 3f)
        {
            isSnapping = false;
            velocity = Vector3.zero;
        }
    }

    private Vector3 AdjustLineOfSight(Vector3 newPosition, Vector3 target)
    {
        if (Physics.Linecast(target, newPosition, out RaycastHit hitInfo, 0))
        {
            velocity = Vector3.zero;
            return hitInfo.point;
        }
        return newPosition;
    }

    private void ApplyPositionDamping(Vector3 targetCenter)
    {
        Vector3 vector = base.transform.rotation * Vector3.back * distance;
        vector.y = 0f;
        Vector3 position = base.transform.position;
        Vector3 vector2 = position - targetCenter;
        vector2.y = 0f;
        Vector3 vector3 = vector2.normalized * vector.magnitude + targetCenter;
        Vector3 vector4 = default(Vector3);
        vector4.x = Mathf.SmoothDamp(position.x, vector3.x, ref velocity.x, smoothLag, maxSpeed);
        vector4.z = Mathf.SmoothDamp(position.z, vector3.z, ref velocity.z, smoothLag, maxSpeed);
        vector4.y = Mathf.SmoothDamp(position.y, targetCenter.y, ref velocity.y, smoothLag, maxSpeed);
        vector4 = AdjustLineOfSight(vector4, targetCenter);
        base.transform.position = vector4;
    }

    private void SetUpRotation(Vector3 centerPos, Vector3 headPos)
    {
        Vector3 position = base.transform.position;
        Vector3 vector = centerPos - position;
        Quaternion lhs = Quaternion.LookRotation(new Vector3(vector.x, 0f, vector.z));
        Vector3 forward = Vector3.forward * distance + Vector3.down * height;
        base.transform.rotation = lhs * Quaternion.LookRotation(forward);
    }

    private float AngleDistance(float a, float b)
    {
        a = Mathf.Repeat(a, 360f);
        b = Mathf.Repeat(b, 360f);
        return Mathf.Abs(b - a);
    }

    public override void GetNextPositionAngle(ref Vector3 position, ref Quaternion angle)
    {
        position = target.position + headOffset + target.transform.forward * (0f - distance);
        angle = Quaternion.LookRotation(target.position + centerOffset - position);
        isSnapping = true;
    }
}
