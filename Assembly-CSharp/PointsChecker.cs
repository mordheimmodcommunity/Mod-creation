using System.Collections.Generic;
using UnityEngine;

public class PointsChecker
{
    public const float CAPSULE_MIN_HEIGHT = 0.6f;

    private const float RAY_RANGE = 100f;

    private const float ANGLE_INCREMENT = 60f;

    protected const float SPHERE_RADIUS = 0.5f;

    public const float ALLY_DIST_OFFSET = 0.2f;

    public const float ENEMY_DIST_OFFSET = -0.22f;

    protected readonly Transform zoneTransform;

    private readonly bool applyOffset;

    public List<Vector3> validPoints = new List<Vector3>();

    public List<UnitController> alliesOnZone = new List<UnitController>();

    public List<UnitController> enemiesOnZone = new List<UnitController>();

    public PointsChecker(Transform transform, bool hasOffset)
    {
        zoneTransform = transform;
        applyOffset = hasOffset;
    }

    public void UpdateControlPoints(UnitController unit, List<UnitController> allUnits)
    {
        validPoints.Clear();
        float floatSqr = Constant.GetFloatSqr(ConstantId.MELEE_RANGE_LARGE);
        float floatSqr2 = Constant.GetFloatSqr(ConstantId.MELEE_RANGE_NORMAL);
        float num = unit.CapsuleRadius / 2f;
        float dist = ((unit.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? Constant.GetFloat(ConstantId.MELEE_RANGE_NORMAL) : Constant.GetFloat(ConstantId.MELEE_RANGE_LARGE)) + 0.2f;
        Vector3 position = zoneTransform.position;
        if (applyOffset)
        {
            position += zoneTransform.forward * (0f - num);
        }
        Vector2 vector = new Vector2(position.x, position.z);
        UpdateUnitsOnActionZone(unit, allUnits, vector, position);
        for (float num2 = 0f; num2 < 360f; num2 += 60f)
        {
            if (!GetPoint(position, num2, dist, out Vector3 pos))
            {
                continue;
            }
            Vector2 vector2 = new Vector2(pos.x, pos.z);
            Vector2 checkDestPoint = vector2 + (vector - vector2) * 100f;
            bool flag = true;
            for (int i = 0; i < allUnits.Count; i++)
            {
                if (!(allUnits[i] == unit) && !alliesOnZone.Contains(allUnits[i]) && !enemiesOnZone.Contains(allUnits[i]))
                {
                    float num3 = (allUnits[i].unit.Data.UnitSizeId != UnitSizeId.LARGE) ? floatSqr2 : floatSqr;
                    if (Vector3.SqrMagnitude(allUnits[i].transform.position - position) < floatSqr * 2f && Vector3.SqrMagnitude(allUnits[i].transform.position - pos) < num3 && PandoraUtils.IsPointInsideEdges(allUnits[i].combatCircle.Edges, vector2, checkDestPoint))
                    {
                        flag = false;
                    }
                }
            }
            if (flag)
            {
                validPoints.Add(pos);
            }
        }
    }

    public virtual bool GetPoint(Vector3 startPoint, float angle, float dist, out Vector3 pos)
    {
        Vector3 forward = Vector3.forward;
        forward = Quaternion.Euler(0f, angle, 0f) * forward;
        forward.Normalize();
        pos = Vector3.zero;
        if (!PandoraUtils.SendCapsule(startPoint, forward, 0.6f, 1.5f, dist, 0.5f))
        {
            pos = startPoint + forward * dist;
            return true;
        }
        return false;
    }

    private void UpdateUnitsOnActionZone(UnitController unit, List<UnitController> allUnits, Vector2 zoneflatPos, Vector3 zonePos)
    {
        alliesOnZone.Clear();
        enemiesOnZone.Clear();
        float floatSqr = Constant.GetFloatSqr(ConstantId.MELEE_RANGE_LARGE);
        float floatSqr2 = Constant.GetFloatSqr(ConstantId.MELEE_RANGE_NORMAL);
        Vector2 vector = default(Vector2);
        for (int num = allUnits.Count - 1; num >= 0; num--)
        {
            if (allUnits[num] != unit)
            {
                Vector3 position = allUnits[num].transform.position;
                float x = position.x;
                Vector3 position2 = allUnits[num].transform.position;
                vector = new Vector2(x, position2.z);
                Vector2 checkDestPoint = zoneflatPos + (vector - zoneflatPos) * 100f;
                float num2 = Vector3.SqrMagnitude(allUnits[num].transform.position - zonePos);
                float num3 = (unit.unit.Data.UnitSizeId != UnitSizeId.LARGE && allUnits[num].unit.Data.UnitSizeId != UnitSizeId.LARGE) ? floatSqr2 : floatSqr;
                if (num2 < 0.1f || (num2 < num3 && PandoraUtils.IsPointInsideEdges(allUnits[num].combatCircle.Edges, vector, checkDestPoint)))
                {
                    if (unit.IsEnemy(allUnits[num]))
                    {
                        enemiesOnZone.Add(allUnits[num]);
                    }
                    else
                    {
                        alliesOnZone.Add(allUnits[num]);
                    }
                }
            }
        }
    }

    public bool IsAvailable()
    {
        if (alliesOnZone.Count == 0 && enemiesOnZone.Count == 0)
        {
            return true;
        }
        for (int i = 0; i < alliesOnZone.Count; i++)
        {
            if (alliesOnZone[i].Engaged || alliesOnZone[i].unit.Data.UnitSizeId == UnitSizeId.LARGE)
            {
                return false;
            }
        }
        for (int j = 0; j < enemiesOnZone.Count; j++)
        {
            if (enemiesOnZone[j].Engaged || enemiesOnZone[j].unit.Data.UnitSizeId == UnitSizeId.LARGE)
            {
                return false;
            }
        }
        return validPoints.Count >= alliesOnZone.Count + enemiesOnZone.Count;
    }

    public bool CanDoAthletic()
    {
        return alliesOnZone.Count == 0 && enemiesOnZone.Count == 0;
    }
}
