using System;
using UnityEngine;

public class EllipsePointsChecker : PointsChecker
{
    private float radiusA;

    private float radiusB;

    public EllipsePointsChecker(Transform transform, bool hasOffset, float radius1, float radius2)
        : base(transform, hasOffset)
    {
        radiusA = radius1;
        radiusB = radius2;
    }

    public override bool GetPoint(Vector3 startPoint, float angle, float dist, out Vector3 pos)
    {
        float f = angle * (MathF.PI / 180f);
        float num = radiusA * radiusB / Mathf.Sqrt(Mathf.Pow(radiusB, 2f) + Mathf.Pow(radiusA, 2f) * Mathf.Pow(Mathf.Tan(f), 2f));
        num *= (((!(0f <= angle) || !(angle < 90f)) && (!(270f < angle) || !(angle <= 360f))) ? 1f : (-1f));
        float z = Mathf.Tan(f) * num;
        Vector3 a = new Vector3(num, 0f, z);
        a = zoneTransform.rotation * -a;
        dist = a.magnitude;
        a /= dist;
        pos = Vector3.zero;
        if (!PandoraUtils.SendCapsule(startPoint, a, 0.6f, 1.5f, dist, 0.5f))
        {
            pos = startPoint + a * dist;
            return true;
        }
        return false;
    }
}
