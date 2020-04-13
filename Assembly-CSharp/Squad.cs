using System.Collections.Generic;
using UnityEngine;

public class Squad
{
    private const float MIN_TARGETS_SQR_DIST = 100f;

    public List<UnitController> members;

    public UnitController targetEnemy;

    public bool targetSpottedReachable;

    public Squad()
    {
        members = new List<UnitController>();
    }

    public void RefineTargetsList(List<UnitController> targets)
    {
        if (targetEnemy == null || targetEnemy.unit.Status == UnitStateId.OUT_OF_ACTION)
        {
            targetEnemy = null;
            return;
        }
        for (int num = targets.Count - 1; num >= 0; num--)
        {
            if (Vector3.SqrMagnitude(targets[num].transform.position - targetEnemy.transform.position) > 100f)
            {
                targets.RemoveAt(num);
            }
        }
        if (targets.Count == 0)
        {
            targets.Add(targetEnemy);
        }
    }

    public void SetTarget(UnitController target, bool spottedReachable)
    {
        if (!(target == null) && (targetEnemy == null || targetEnemy.unit.Status == UnitStateId.OUT_OF_ACTION || (targetEnemy != null && !targetSpottedReachable && target != null && spottedReachable)))
        {
            targetEnemy = target;
            targetSpottedReachable = spottedReachable;
        }
    }

    public void RemoveDeadMembers()
    {
        for (int num = members.Count - 1; num >= 0; num--)
        {
            if (members[num].unit.Status == UnitStateId.OUT_OF_ACTION)
            {
                members.RemoveAt(num);
            }
        }
    }

    public bool LoneLostLastMember()
    {
        if (members.Count == 1 && !members[0].Engaged)
        {
            List<UnitController> aliveEnemies = PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(members[0].GetWarband().idx);
            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                if (Vector3.SqrMagnitude(members[0].transform.position - aliveEnemies[i].transform.position) < 100f)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
}
