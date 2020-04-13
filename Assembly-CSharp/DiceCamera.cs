using UnityEngine;

public class DiceCamera : CameraBase
{
    private Transform targetPosition;

    private void Update()
    {
    }

    public override void GetNextPositionAngle(ref Vector3 position, ref Quaternion angle)
    {
        position = GetPosition();
        angle = GetAngle();
    }

    private Vector3 GetPosition()
    {
        return base.transform.position;
    }

    private Quaternion GetAngle()
    {
        return base.transform.rotation;
    }

    public override void SetTarget(Transform target)
    {
        targetPosition = target;
    }

    public override Transform GetTarget()
    {
        return targetPosition;
    }
}
